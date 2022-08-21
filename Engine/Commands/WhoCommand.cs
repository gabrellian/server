using Data;
using Engine.Net;
using Spectre.Console;
using System.Text.RegularExpressions;
using Spectre.Console.Advanced;

namespace Engine.Commands;

public class WhoCommand : BaseCommand
{
    private IPlayfieldService _playfieldService;

    public WhoCommand(IPlayfieldService playfieldService)
    {
        _playfieldService = playfieldService;
    }

    public override async Task Handle()
    {
        Table tbl = new Table();
        tbl.AddColumn("Player");
        tbl.AddColumn("Playfield");
        tbl.AddColumn("Room");
        tbl.AddColumn("Status");

        Session.SendLine($"Players Online:");
        foreach (GameSession playerSession in _playfieldService.Players)
        {
            tbl.AddRow(new [] {
                playerSession.CurrentPlayer.Nickname,
                playerSession.CurrentPlayfield.DisplayName,
                playerSession.CurrentRoom.DisplayName,
                "[green]ONLINE[/]"
            });
            //Session.SendLine($"  - {playerSession.CurrentPlayer.Nickname} [{playerSession.CurrentRoom.DisplayName}]");
        }
        var ansi = Spectre.Console.Advanced.AnsiConsoleExtensions.ToAnsi(AnsiConsole.Console, tbl);
        Session.SendLine(ansi);
    }
}
