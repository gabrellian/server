using Data;
using Engine.Net;
using Spectre.Console;
using System.Text.RegularExpressions;
using Spectre.Console.Advanced;

namespace Engine.Commands;

public class SayCommand : BaseCommand
{
    private IPlayfieldService _playfieldService;

    public SayCommand(IPlayfieldService playfieldService) { _playfieldService = playfieldService; }

    public override async Task Handle()
    {
        var channel = RawCommand.Substring(0, RawCommand.IndexOf(" "));
        var msg = RawCommand.Substring(RawCommand.IndexOf(" "));
        switch (channel.ToLower())
        {
            case "sl":
            case "say":
                foreach (var p in Session.CurrentRoom.Players)
                {
                    if (p.Id != Session.Id)
                    {
                        p.SendLine();
                    }
                    p.SendLine($"([yellow]{Session.CurrentPlayer.Nickname}[/]): {msg.TrimStart().TrimEnd()}".ToAnsi(), showPrompt: false);
                }
                break;
            case "sg":
            case "global":
                foreach (var p in _playfieldService.Players)
                {
                    if (p.Id != Session.Id)
                    {
                        p.SendLine();
                    }
                    p.SendLine($"(Global | [yellow]{Session.CurrentPlayer.Nickname}[/]): {msg.TrimStart().TrimEnd()}".ToAnsi(), showPrompt: false);
                }
                break;
        }
    }
}
