using NUnit.Framework;
using System.Threading;
using System.Windows.Documents;
using Various.Wpf.Controls.TextBlocks;

namespace Various.Wpf.Tests;

[TestFixture]
internal class HyperlinkTextBlockTests
{
    [Test]
    public void DefaultTextValue_ReturnsNull_TextBlockInstantiated()
    {
        var thread = new Thread(() =>
        {
            // Arrange
            var textBlock = new HyperlinkTextBlock();

            // Act
            var result = textBlock.Text;

            // Assert
            Assert.That(result, Is.Null);
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    [Test]
    public void TextProperty_SetsAndGetsValue_TextBlockInstantiated()
    {
        var thread = new Thread(() =>
        {
            // Arrange
            var textBlock = new HyperlinkTextBlock();

            // Act
            textBlock.Text = "http://www.example.com";
            var result = textBlock.Text;

            // Assert
            Assert.That(result, Is.EqualTo("http://www.example.com"));
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    [Test]
    public void OnTextChanged_RecomputesInlines_TextBlockInstantiated()
    {
        var thread = new Thread(() =>
        {
            // Arrange
            var textBlock = new HyperlinkTextBlock();

            // Act
            textBlock.Text = "http://www.example.com";

            // Assert
            Assert.That(textBlock.Inlines.Count, Is.EqualTo(1));
            Assert.That(textBlock.Inlines.FirstInline, Is.TypeOf<Hyperlink>());
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
