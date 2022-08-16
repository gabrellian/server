using Data;
using System.Text.RegularExpressions;

namespace Engine.Commands;

public class WhoCommand : BaseCommand
{

    public WhoCommand()
    {
        
    }

    public override async Task Handle()
    {
        Session.SendLine($"Character Profile:");
        Session.SendLine($"------------------");
        Session.SendLine($"> Name:   {Session.CurrentPlayer.Nickname}");
        Session.SendLine($"------------------");
    }
}
