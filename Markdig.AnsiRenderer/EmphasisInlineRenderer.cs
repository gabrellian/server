using Markdig.Syntax.Inlines;

namespace Markdig.AnsiRenderer
{
    public class EmphasisInlineRenderer : AnsiObjectRenderer<EmphasisInline>
    {
        protected override void Write(AnsiRenderer renderer, EmphasisInline obj)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            switch (obj.DelimiterChar)
            {
                case '_':
                    renderer.Write($"[italic]");
                    break;
                case '~':
                    renderer.Write("[strikethrough]");
                    break;
                case '=':
                    if (obj.DelimiterCount == 1)
                        renderer.Write("[slowblink]");
                    if (obj.DelimiterCount == 2)
                        renderer.Write("[rapidblink]");
                    break;
                case '*':
                    if (obj.DelimiterCount == 1)
                        renderer.Write($"[italic]");
                    if (obj.DelimiterCount == 2)
                        renderer.Write($"[bold]");
                    break;
            }
            renderer.WriteChildren(obj);
            renderer.Write($"[/]");
        }
    }
}