using Data;
using Engine.Net;
using Spectre.Console;
using System.Text.RegularExpressions;
using Spectre.Console.Advanced;
using Engine.Extendable;

namespace Engine.Commands;

public class WhoCommand : BaseCommand, IWhoCommand
{
    private IPlayfieldService _playfieldService;

    public WhoCommand(IPlayfieldService playfieldService)
    {
        _playfieldService = playfieldService;
    }

    public override async Task Handle()
    {
        Table tbl = new Table();
        tbl.AddColumn("[b]Player[/]");
        tbl.AddColumn("[b]Playfield[/]");
        tbl.AddColumn("[b]Room[/]");
        tbl.AddColumn("[b]Status[/]");

        Session.SendLine($"[b]Players Online:[/]".ToAnsi());
        foreach (GameSession playerSession in _playfieldService.Players)
        {
            tbl.AddRow(new [] {
                playerSession.CurrentPlayer.Nickname,
                playerSession.CurrentPlayfield.DisplayName,
                playerSession.CurrentRoom.DisplayName,
                "[green]ONLINE[/]"
            });
        }
        Session.SendLine(tbl, showPrompt: true);
    }
}
