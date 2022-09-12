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
using Engine.Extendable;
using SampleServer;
using System.Text.Json;

var host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((ctx, config) => config
        .AddEnvironmentVariables())
    .Listen(System.Net.IPAddress.Any, 3000)
    .ConfigureServices((ctx, services) => services
        .AddSingleton<ICommandFactory, CommandFactory>()
        .AddSingleton<IPlayfieldService, PlayfieldService>()
        .AddSingleton<IPlayfieldDefinitionRepo, FileSystem.PlayfieldDefinitionRepo>()
        .AddSingleton<IHelpFilesRepo, FileSystem.HelpFileRepo>()
        .AddSingleton<GameSessions>()
        .AddSingleton<ICharacterStatBuilder, CharacterStatBuilder>()
        .AddSingleton<IPlayerCharacterRepo, FileSystem.PlayerCharacterRepo>()
        .AddScoped<WhoCommand>()
        .AddSingleton(services =>
        {
            var jsonOptions = new JsonSerializerOptions();
            jsonOptions.WriteIndented = true;
            jsonOptions.Converters.Add(new StatBlockConverter());
            return jsonOptions;
        })
        .AddSingleton<SessionInitializer>(serverServices =>
            (session) => new ServiceCollection()
                .AddSingleton(session)
                .AddSingleton<IPlayfieldService>(serverServices.GetService<IPlayfieldService>())
                .AddSingleton<IMainMenu, SM_MainMenu>()
                .AddSingleton(serverServices.GetService<IConfiguration>())
                .AddSingleton<ICommandFactory, CommandFactory>()
                .AddSingleton<GameSessions>(serverServices.GetService<GameSessions>())
                .AddTransient<WhoCommand>()
                .AddTransient<HelpCommand>()
                .AddTransient<LookCommand>()
                .AddTransient<IStatCommand, StatCommand>()
                .AddTransient<SayCommand>()
                .AddTransient<CharacterCommand>()
                .AddSingleton<JsonSerializerOptions>(serverServices.GetService<JsonSerializerOptions>())
                .AddSingleton<ICharacterStatBuilder>(serverServices.GetService<ICharacterStatBuilder>())
                .AddSingleton<IPlayerCharacterRepo>(serverServices.GetService<IPlayerCharacterRepo>())
                .AddSingleton<IPlayfieldDefinitionRepo>(serverServices.GetService<IPlayfieldDefinitionRepo>())
                .AddSingleton<IHelpFilesRepo>(serverServices.GetService<IHelpFilesRepo>())
        )
        .AddHostedService<GameServerHost>())
    .Build();
try
{
    await host.StartAsync();
    Console.WriteLine("[CTRL-C] To Exit Serrver");
    await host.WaitForShutdownAsync();
}catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}
