using Engine.StateMachines;
using Engine.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCoreServer;
using System;
using System.Net;
using System.Net.Sockets;

namespace Engine.Net;

public class GameServerHost : IHostedService
{
    private GameServer _server;
    private IPlayfieldService _playfieldService;

    public GameServerHost(IConfiguration config, IPlayfieldService playfieldService, IServiceProvider services)
    {
        _server = new GameServer(
            IPAddress.Parse(config.GetValue<string>("ServerAddress")),
            config.GetValue<int>("ServerPort"),
            services);
        _playfieldService = playfieldService;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _playfieldService.Initialize();
        _server.Start();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _server.Stop();
        return Task.CompletedTask;
    }
}
public class GameServer : TcpServer
{
    private List<PlayfieldInstance> _playfields = new List<PlayfieldInstance>();
    private IServiceProvider _serverServices;

    public GameServer(IPAddress address, int port, IServiceProvider serverServices) : base(address, port)
    {
        _serverServices = serverServices;
    }

    protected override TcpSession CreateSession()
    {
        var session = new GameSession(this);
        
        session.RegisterServiceProvider(_serverServices.GetService<SessionInitializer>().Invoke(session).BuildServiceProvider());

        return session;
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Server caught an error with code {error}");
    }

}

public static class GameServerBuilderExtensions
{
    public static IHostBuilder Listen(this IHostBuilder builder, IPAddress address, int port)
     => builder.ConfigureAppConfiguration((ctx, config) => config.AddInMemoryCollection(new[] {
            new KeyValuePair<string, string>("ServerAddress", address.ToString()),
            new KeyValuePair<string, string>("ServerPort", port.ToString())
        }));

    public static IHostBuilder AddMainMenu<T>(this IHostBuilder builder) where T : StatefulContext
        => builder.ConfigureServices((ctx, svc) => svc.AddScoped(typeof(IMainMenu), typeof(T)));

}
