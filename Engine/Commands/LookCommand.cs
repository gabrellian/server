using Spectre.Console;

namespace Engine.Commands;

public class LookCommand : BaseCommand
{
    private IPlayfieldService _playfieldService;

    public LookCommand(IPlayfieldService playfieldService)
    {
        _playfieldService = playfieldService;
    }

    public override async Task Handle()
    {
        Session.SendLook();
    }
}