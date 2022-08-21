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

        Session.SendLine($"Character Profile");
        Session.SendLine($"  - Name:   {Session.CurrentPlayer.Nickname}");
    }
}
