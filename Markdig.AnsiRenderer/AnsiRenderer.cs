using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Markdig.Syntax;
using Spectre.Console;
using S = Spectre.Console;
using Table = Markdig.Extensions.Tables.Table;
using TableRow = Markdig.Extensions.Tables.TableRow;

namespace Markdig.AnsiRenderer
{
    public class AnsiRenderer : TextRendererBase<AnsiRenderer>
    {
        public AnsiRenderer(TextWriter writer) : base(writer)
        {
            ObjectRenderers.Add(new ParagraphRenderer());
            ObjectRenderers.Add(new EmphasisInlineRenderer());
            ObjectRenderers.Add(new DelimiterInlineRenderer());
            ObjectRenderers.Add(new LiteralInlineRenderer());
            ObjectRenderers.Add(new TableRenderer());
        }
    }
    public abstract class AnsiObjectRenderer<TObject> : MarkdownObjectRenderer<AnsiRenderer, TObject>
        where TObject : MarkdownObject
    {

    }

    public class TableRenderer : AnsiObjectRenderer<Table>
    {
        protected override void Write(AnsiRenderer renderer, Table obj)
        {
            var tbl = new S.Table();
            var ix = 0;
            foreach (TableRow row in obj)
            {
                if (ix == 0)
                {
                    // It's the header row
                    foreach (TableCell cell in row)
                    {
                        var header = (cell.FirstOrDefault() as ParagraphBlock).Inline.FirstChild.ToString();
                        using var childWriter = new StringWriter();
                        var childRenderer = new AnsiRenderer(childWriter);
                        childRenderer.Render(cell.FirstOrDefault());
                        tbl.AddColumn(childWriter.ToString());
                    }
                }
                else
                {
                    List<string> cells = new List<string>();
                    foreach (TableCell cell in row)
                    {
                        using var childWriter = new StringWriter();
                        var childRenderer = new AnsiRenderer(childWriter);
                        childRenderer.Render(cell.FirstOrDefault());
                        cells.Add(childWriter.ToString());
                    }
                    tbl.AddRow(cells.ToArray());
                }
                ix++;
            }
            renderer.Write(tbl.ToAnsi());
        }
    }
    public static class Ansi
    {
        public static string ToAnsi(this string msg, Style style = null) => new Markup(msg, style).ToAnsi();
        public static string ToAnsi(this Spectre.Console.Rendering.IRenderable renderable)
            => Spectre.Console.Advanced.AnsiConsoleExtensions.ToAnsi(AnsiConsole.Console, renderable);

    }
}