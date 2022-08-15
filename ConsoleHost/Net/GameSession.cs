using NetCoreServer;
using System.Text;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using ConsoleHost.Commands;
using Data.Models;
using ConsoleHost.Utils;
using ConsoleHost.StateMachines;

namespace ConsoleHost.Net;

public class GameSession : TcpSession
{
    private List<byte> _buffer = new List<byte>();
    private IServiceScope _scopedServices;
    public PlayerCharacter CurrentPlayer { get; private set; }
    public List<StatefulContext> StatefulContexts { get; set; } = new List<StatefulContext>();
    public GameSession(TcpServer server, IServiceScope scope) : base(server)
    {
        _scopedServices = scope;
    }

    public T GetService<T>() => _scopedServices.ServiceProvider.GetService<T>();
    public void SendLine(string msg) => Send(msg + "\r\n");
    public bool SendLineAsync(string msg) => SendAsync(msg + "\r\n");

    protected override void OnConnected()
    {
        StatefulContexts.Add(new SM_MainMenu(this));
        base.OnConnected();
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
                    foreach (var sc in StatefulContexts)
                    {
                        sc.SetState(sc.CurrentState.OnCommand(Encoding.UTF8.GetString(_buffer.ToArray()).TrimEnd().TrimStart()).Result);
                    }
                    ProcessCommand(Encoding.UTF8.GetString(_buffer.ToArray()));
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
        _scopedServices.ServiceProvider.GetService<ICommandFactory>().Match(raw, this);
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

    public void DetachPlayer()
    {
        CurrentPlayer = null;
    }
}
