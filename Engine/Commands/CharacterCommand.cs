using Data;
using System.Text.RegularExpressions;

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
