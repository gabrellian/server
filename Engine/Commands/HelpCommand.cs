using Data;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace Engine.Commands;

public class HelpCommand : BaseCommand
{
    private IHelpFilesRepo _helpFiles;

    public HelpCommand(IHelpFilesRepo helpFiles)
    {
        _helpFiles = helpFiles;
    }

    public override async Task Handle()
    {
        var page = "help " + string.Join(" ", RawCommand.Split(" ").Skip(1));
        var data = await _helpFiles.GetHelpPage(page.TrimEnd());
        var tbl = new Table().AddColumn($"Help -> {page}").AddRow(new [] {data});
        Session.SendLine(Spectre.Console.Advanced.AnsiConsoleExtensions.ToAnsi(AnsiConsole.Console, tbl));
    }
}
