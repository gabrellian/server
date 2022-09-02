using Markdig.Syntax;

namespace Markdig.AnsiRenderer
{
    public class ParagraphRenderer : AnsiObjectRenderer<ParagraphBlock>
    {
        protected override void Write(AnsiRenderer renderer, ParagraphBlock obj)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            if (!renderer.IsFirstInContainer)
            {
                renderer.EnsureLine();
            }
            renderer.WriteLeafInline(obj);
        }
    }
}