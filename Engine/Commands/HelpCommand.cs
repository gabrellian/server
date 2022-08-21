using Data;
using System.Text.RegularExpressions;

namespace Engine.Commands;

public class HelpCommand : BaseCommand
{

    public HelpCommand()
    {
        
    }

    public override async Task Handle()
    {
        Session.SendLine($"Help ");
        Session.SendLine($"  - logout       - signs off the current character");
        Session.SendLine($"  - character    - lists details about yourself");
    }
}
