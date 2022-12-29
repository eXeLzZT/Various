using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace Various.Utils.Tests;

[TestFixture]
internal class StringExtensionsTests
{
    [Test]
    public void Match_ReturnsMatch_WhenTextContainsUrlAtPosition()
    {
        // Arrange
        string text = "This is a test string with a URL: https://www.example.com";
        int position = 23;

        // Act
        Match result = text.Match(position);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo("https://www.example.com"));
    }

    [Test]
    public void Match_ReturnsMatch_WhenTextContainsMultipleUrlsAndPositionIsInMiddleOfOne()
    {
        // Arrange
        string text = "This is a test string with multiple URLs: https://www.example.com and https://www.example.net";
        int position = 44;
        Match result = text.Match(position);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo("https://www.example.net"));
    }

    [Test]
    public void Match_ReturnsMatch_WhenTextContainsUrlAtBeginningOfString()
    {
        // Arrange
        string text = "https://www.example.com This is a test string with a URL";
        int position = 0;

        // Act
        Match result = text.Match(position);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo("https://www.example.com"));
    }

    [Test]
    public void Match_ReturnsMatch_WhenTextContainsUrlAtEndOfString()
    {
        // Arrange 
        string text = "This is a test string with a URL: https://www.example.com";
        int position = text.Length - 23;

        // Act
        Match result = text.Match(position);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo("https://www.example.com"));
    }

    [Test]
    public void Match_ReturnsNoMatch_WhenTextDoesNotContainUrlAtPosition()
    {
        // Arrange
        string text = "This is a test string with no URL";
        int position = 5;

        // Act
        Match result = text.Match(position);

        // Assert
        Assert.That(result.Success, Is.False);
    }
}