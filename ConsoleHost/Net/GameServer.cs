using Microsoft.Extensions.DependencyInjection;
using NetCoreServer;
using System;
using System.Net;
using System.Net.Sockets;

namespace ConsoleHost.Net;

public class GameServer : TcpServer
{
    private IServiceProvider _services;
    public GameServer(IPAddress address, int port, IServiceProvider services) : base(address, port)
    {
        _services = services;
    }

    protected override TcpSession CreateSession() 
        => new GameSession(this, _services.CreateScope()); 
        

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Server caught an error with code {error}");
    }
}
