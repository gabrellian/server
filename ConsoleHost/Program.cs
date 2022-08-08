using ConsoleHost.Commands;
using ConsoleHost.Net;
using Data;
using FileSystem = Data.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

var config = new ConfigurationBuilder()
    .AddJsonFile("server.json", true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(config);
services.AddSingleton<ICommandFactory, CommandFactory>();
services.AddSingleton<IPlayerCharacterRepo , FileSystem.PlayerCharacterRepo>();

services.AddScoped<LoginCommand>();

var provider = services.BuildServiceProvider();

var server = new GameServer(System.Net.IPAddress.Any, 3000, provider);

server.Start();

Console.WriteLine("[CTRL-C] To Exit Server");
while (true) ;