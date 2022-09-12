using Data;
using Data.Models;
using Engine.Extendable;
using Spectre.Console;
using System.Text.RegularExpressions;
using Spectre = Spectre.Console;
namespace Engine.Commands;

public class CharacterCommand : BaseCommand, ICharacterCommand
{

    public CharacterCommand()
    {

    }

    public override async Task Handle()
    {
        var args = this.RawCommand.Split(" ");
        if (args.Contains("json"))
        {
            Session.SendLine(System.Text.Json.JsonSerializer.Serialize(Session.CurrentPlayer, typeof(PlayerCharacter), new System.Text.Json.JsonSerializerOptions()
            {
                WriteIndented = true
            }));
        }
        else
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
}
