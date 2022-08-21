﻿using NetCoreServer;
using System.Text;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Engine.Commands;
using Data.Models;
using Engine.Utils;
using Engine.StateMachines;

namespace Engine.Net;
public delegate Microsoft.Extensions.DependencyInjection.IServiceCollection SessionInitializer(Engine.Net.GameSession session);
public class GameSession : TcpSession
{
    private List<byte> _buffer = new List<byte>();
    private IServiceProvider _services;
    public PlayerCharacter CurrentPlayer { get; private set; }
    public List<IStatefulContext> StatefulContexts { get; set; } = new List<IStatefulContext>();
    private GameServer _server => _server as GameServer;

    public Room CurrentRoom { get; private set; }
    public Playfield CurrentPlayfield { get; private set; }

    public GameSession(TcpServer server) : base(server)
    {
    }
    public void RegisterServiceProvider(IServiceProvider provider) => _services = provider;

    public T GetService<T>() => _services.GetService<T>();
    public void SendLine(string msg = "") => Send(msg + "\r\n");

    protected override void OnConnected()
    {
        StatefulContexts.Add(_services.GetService<IMainMenu>());
        base.OnConnected();
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
                    SendLine();
                    foreach (var sc in StatefulContexts)
                    {
                        sc.OnCommand(cmd).Wait();
                    }
                    ProcessCommand(cmd);
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

    protected void ProcessCommand(string raw)
    {
        _services.GetService<ICommandFactory>().Match(raw, this);
        _buffer.Clear();
    }

    public void AttachPlayer(PlayerCharacter player)
    {
        CurrentPlayer = player;
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Session caught an error with code {error}");
    }

    public async Task ChangeRoom(string path)
    {
        var (pf, rm) = await _services.GetService<IPlayfieldService>().GetRoom(path);
        CurrentRoom = rm;
        CurrentPlayfield = pf;
        await rm.AddPlayer(this);
    }

    public void DetachPlayer()
    {
        CurrentPlayer = null;
    }
}
