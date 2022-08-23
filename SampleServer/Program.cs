using Engine.Commands;
using Engine.Net;
using Data;
using FileSystem = Data.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Engine;
using Microsoft.Extensions.Hosting;
using Engine.StateMachines;

var host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((ctx, config) => config
        .AddEnvironmentVariables())
    .Listen(System.Net.IPAddress.Any, 3000)
    .ConfigureServices((ctx, services) => services
        .AddSingleton<ICommandFactory, CommandFactory>()
        .AddSingleton<IPlayfieldService, PlayfieldService>()
        .AddSingleton<IPlayfieldDefinitionRepo, FileSystem.PlayfieldDefinitionRepo>()
        .AddSingleton<IHelpFilesRepo, FileSystem.HelpFileRepo>()
        .AddSingleton<IPlayerCharacterRepo, FileSystem.PlayerCharacterRepo>()
        .AddScoped<WhoCommand>()
        .AddSingleton<SessionInitializer>(serverServices =>
            (session) => new ServiceCollection()
                .AddSingleton(session)
                .AddSingleton<IPlayfieldService>(serverServices.GetService<IPlayfieldService>())
                .AddSingleton<IMainMenu, SM_MainMenu>()
                .AddSingleton(serverServices.GetService<IConfiguration>())
                .AddSingleton<ICommandFactory, CommandFactory>()
                .AddTransient<WhoCommand>()
                .AddTransient<HelpCommand>()
                .AddTransient<LookCommand>()
                .AddTransient<CharacterCommand>()
                .AddSingleton<IPlayerCharacterRepo>(serverServices.GetService<IPlayerCharacterRepo>())
                .AddSingleton<IPlayfieldDefinitionRepo>(serverServices.GetService<IPlayfieldDefinitionRepo>())
                .AddSingleton<IHelpFilesRepo>(serverServices.GetService<IHelpFilesRepo>())
        )
        .AddHostedService<GameServerHost>())
    .Build();

await host.StartAsync();
Console.WriteLine("[CTRL-C] To Exit Server");
await host.WaitForShutdownAsync();
