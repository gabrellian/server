using Data;
using Spectre.Console;
using System.Text.RegularExpressions;
using Spectre = Spectre.Console;
namespace Engine.Commands;

public class CharacterCommand : BaseCommand
{

    public CharacterCommand()
    {

    }

    public override async Task Handle()
    {
        Session.SendLine($"[b]Character Profile:[/]".ToAnsi());
        Session.SendLine(new Table()
            .AddColumns("Stat", "Value")
            .AddRow("Name", Session.CurrentPlayer.Nickname)
            .AddEmptyRow()
            .AddRow("Location", $"{Session.CurrentPlayfield.DisplayName} - {Session.CurrentRoom.DisplayName}")
            , showPrompt: true);
    }
}
