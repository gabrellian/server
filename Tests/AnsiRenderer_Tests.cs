using Markdig;
using Markdig.AnsiRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class AnsiRenderer_Tests
    {
        private string RenderAsAnsi(string markdown)
        {
            using var writer = new StringWriter();
            var renderer = new AnsiRenderer(writer);

            var parsed = Markdig.Markdown.Parse(markdown, new MarkdownPipelineBuilder()
                .UsePipeTables()
                .UseGridTables()
                .UseEmphasisExtras()
                .Build());
            renderer.Render(parsed);
            string final = "";
            try
            {
                final = writer.ToString().Replace("[", "[[").Replace("]", "]]").ToAnsi();
                final = final.ToAnsi();
            }
            catch (InvalidOperationException) { }
            return final;
        }
        [TestMethod]
        public void Bold_Test()
        {
            Assert.AreEqual("\u001b[1mHello\u001b[0m", RenderAsAnsi("**Hello**"));
        }
        [TestMethod]
        public void Italic_Test()
        {
            Assert.AreEqual("[3mHello[0m", RenderAsAnsi("_Hello_"));
            Assert.AreEqual("[3mHello[0m", RenderAsAnsi("*Hello*"));
        }
        [TestMethod]
        public void Blink_Test()
        {
            Assert.AreEqual("[6mHello[0m", RenderAsAnsi("==Hello=="));
        }
        [TestMethod]
        public void PlainText_Test()
        {
            Assert.AreEqual("Hi Kevin!", RenderAsAnsi("Hi Kevin!"));
        }
        [TestMethod]
        public void BasicTable_Test()
        {
            var table =
                "a | b" + "\r\n" +
                "- | -" + "\r\n" +
                "0 | 1" + "\r\n";
            Assert.AreEqual(
              @"┌───┬───┐" + Environment.NewLine
            + @"│ a │ b │" + Environment.NewLine
            + @"├───┼───┤" + Environment.NewLine
            + @"│ 0 │ 1 │" + Environment.NewLine
            + @"└───┴───┘" + Environment.NewLine, RenderAsAnsi(table));
        }
        [TestMethod]
        public void BiggerTable_Test()
        {
            var table =
                "a | b" + "\r\n" +
                "- | -" + "\r\n" +
                "0 | Some more text goes here if the table gets bigger" + "\r\n";
            Assert.AreEqual(
              @"┌───┬───────────────────────────────────────────────────┐" + Environment.NewLine
            + @"│ a │ b                                                 │" + Environment.NewLine
            + @"├───┼───────────────────────────────────────────────────┤" + Environment.NewLine
            + @"│ 0 │ Some more text goes here if the table gets bigger │" + Environment.NewLine
            + @"└───┴───────────────────────────────────────────────────┘" + Environment.NewLine, RenderAsAnsi(table));
        }
        [TestMethod]
        public void BasicTableWithBoldHeader_Test()
        {
            var table =
                "**a** | b" + Environment.NewLine +
                "- | -" + Environment.NewLine +
                "0 | 1" + Environment.NewLine;
            var rendered = RenderAsAnsi(table);
            System.IO.File.WriteAllText("c:\\src\\render.txt", rendered);
            Assert.AreEqual(
            "┌───┬───┐" + Environment.NewLine +
            "│ [1ma[0m │ b │" + Environment.NewLine +
            "├───┼───┤" + Environment.NewLine +
            "│ 0 │ 1 │" + Environment.NewLine +
            "└───┴───┘" + Environment.NewLine, rendered);
        }
    }
}
