using Markdig.Syntax.Inlines;

namespace Markdig.AnsiRenderer
{
    public class DelimiterInlineRenderer : AnsiObjectRenderer<DelimiterInline>
    {
        protected override void Write(AnsiRenderer renderer, DelimiterInline obj)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            renderer.Write(obj.ToLiteral());
            renderer.WriteChildren(obj);
        }
    }
}