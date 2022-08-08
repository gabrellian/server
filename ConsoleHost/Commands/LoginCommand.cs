using Data;
using System.Text.RegularExpressions;

namespace ConsoleHost.Commands;

public class LoginCommand : BaseCommand
{
    private IPlayerCharacterRepo  _pcRepo;

    public string Username { get; set; }
    public string Password { get; set; }

    public LoginCommand(IPlayerCharacterRepo pcRepo)
    {
        _pcRepo = pcRepo;
    }

    public override async Task Handle()
    {
        var pc = await _pcRepo.GetPlayer(Username);
        if (pc == null)
        {
            Session.Send($"Unknown character:  {Username}");
            return;
        }
        Session.AttachPlayer(pc);
        Session.Send($"Welcome {Session.CurrentPlayer.Nickname}");
    }
}
