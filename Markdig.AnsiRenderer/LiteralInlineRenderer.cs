using Markdig.Syntax.Inlines;

namespace Markdig.AnsiRenderer
{
    public class LiteralInlineRenderer : AnsiObjectRenderer<LiteralInline>
    {
        protected override void Write(AnsiRenderer renderer, LiteralInline obj)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            if (obj.Content.IsEmpty)
                return;

            renderer.Write(ref obj.Content);
        }
    }
}