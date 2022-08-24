using Spectre.Console;

public static class Ansi
{
    public static string ToAnsi(this string msg, Style style = null) => new Markup(msg, style).ToAnsi();
    public static string ToAnsi(this Spectre.Console.Rendering.IRenderable renderable)
        => Spectre.Console.Advanced.AnsiConsoleExtensions.ToAnsi(AnsiConsole.Console, renderable);
    
}