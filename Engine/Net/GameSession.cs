using NetCoreServer;
using System.Text;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Engine.Commands;
using Data.Models;
using Engine.Core;
using Engine.StateMachines;
using Spectre.Console.Rendering;
using Spectre.Console;

namespace Engine.Net;
public delegate Microsoft.Extensions.DependencyInjection.IServiceCollection SessionInitializer(Engine.Net.GameSession session);
public class GameSession : TcpSession
{
    private List<byte> _buffer = new List<byte>();
    private IServiceProvider _services;
    public virtual PlayerCharacter CurrentPlayer { get; private set; }
    public virtual List<IStatefulContext> StatefulContexts { get; set; } = new List<IStatefulContext>();
    private GameServer _server => _server as GameServer;

    public virtual RoomInstance CurrentRoom { get; private set; }
    public virtual PlayfieldInstance CurrentPlayfield { get; private set; }

    public GameSession(TcpServer server) : base(server)
    {
    }
    public void RegisterServiceProvider(IServiceProvider provider) => _services = provider;

    public virtual T GetService<T>() => _services.GetService<T>();
    public virtual void SendLine(string msg = "", bool showPrompt = false)
    {
        Send(msg + "\r\n");
        if (showPrompt)
        {
            SendPrompt();
        }
    }
    public void SendLine(IRenderable msg, bool showPrompt = false)
        => Send(msg.ToAnsi() + "\r\n");


    protected override void OnConnected()
    {
        StatefulContexts.Add(_services.GetService<IMainMenu>());
        base.OnConnected();
    }
    protected override void OnDisconnected()
    {
        var _ = DetachPlayer();
    }
    public virtual async Task OnTick(TimeSpan delta)
    {
        // Pass commands onto PF/Room specific logic and 
        foreach (var ctx in StatefulContexts) await ctx.OnTick(delta);
    }

    protected override void OnReceived(byte[] inbound, long offset, long size)
    {
        foreach (byte b in inbound)
            switch (b)
            {
                case 27:
                    Send("Goodbye!");
                    Disconnect();
                    break;
                case 13:
                    var cmd = Encoding.UTF8.GetString(_buffer.ToArray()).TrimEnd().TrimStart();
                    if (!string.IsNullOrEmpty(cmd))
                    {
                        foreach (var sc in StatefulContexts)
                        {
                            sc.OnCommand(cmd).Wait();
                        }
                        ProcessCommand(cmd);
                    }
                    break;
                case 32:
                    _buffer.Add(b);
                    break;
                default:
                    if (b >= 65 && b <= 90 || b >= 97 && b <= 122)
                    {
                        _buffer.Add(b);
                    }
                    break;
            }
    }

    public virtual void SendPrompt()
    {
        Send("> ");
    }

    protected void ProcessCommand(string raw)
    {
        var cmd = GetService<ICommandFactory>().Match(raw, this);

        if (CurrentPlayfield != null)
        {
            SendPrompt();
        }

        _buffer.Clear();

    }

    public virtual void AttachPlayer(PlayerCharacter player)
    {
        CurrentPlayer = player;
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Session caught an error with code {error}");
    }

    public virtual async Task ChangeRoom(string path)
    {
        var (pf, rm) = await GetService<IPlayfieldService>().GetRoom(path);
        CurrentRoom = rm;
        CurrentPlayfield = pf;
        await rm.AddPlayer(this);
    }

    public virtual async Task DetachPlayer()
    {
        CurrentPlayer = null;
        await CurrentRoom.RemovePlayer(this);
        CurrentRoom = null;
        CurrentPlayfield = null;
    }

    public void SendLook() =>
        SendLine(new Table()
            .AddColumn(CurrentRoom.DisplayName)
            .AddRow(CurrentRoom.FullDescription), showPrompt: true);
}
