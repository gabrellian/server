using Data;
using Engine.Net;
using System.Text.RegularExpressions;

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
        Session.SendLine($"Players Online:");
        foreach (GameSession playerSession in _playfieldService.Players)
        {
            Session.SendLine($"  - {playerSession.CurrentPlayer.Nickname} [{playerSession.CurrentRoom.DisplayName}]");
        }
    }
}
