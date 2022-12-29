using NUnit.Framework;
using System;

namespace Various.Utils.Tests;

[TestFixture]
internal class PasswordUtilsTests
{
    [Test]
    public void GenerateSalt_ReturnsValidSalt_NumberSaltSpecified()
    {
        // Arrange
        int numberSalt = 16;

        // Act
        string salt = PasswordUtils.GenerateSalt(numberSalt);

        // Assert
        Assert.That(salt, Is.Not.Null);
        Assert.That(Convert.FromBase64String(salt), Has.Length.EqualTo(numberSalt));
    }

    [Test]
    public void HashPassword_ReturnsValidHash_ValidInputs()
    {
        // Arrange
        string password = "password123";
        string salt = PasswordUtils.GenerateSalt();
        int numberInterations = 12288;
        int numberHash = 32;

        // Act
        string hash = PasswordUtils.HashPassword(password, salt, numberInterations, numberHash);

        // Assert
        Assert.That(hash, Is.Not.Null);
        Assert.That(Convert.FromBase64String(hash), Has.Length.EqualTo(numberHash));
    }

    [Test]
    public void IsPasswordValid_ReturnsTrue_ValidPassword()
    {
        // Arrange
        string password = "password123";
        string salt = PasswordUtils.GenerateSalt();
        string hashedPassword = PasswordUtils.HashPassword(password, salt);

        // Act
        var isValid = PasswordUtils.IsPasswordValid(password, hashedPassword, salt);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsPasswordValid_ReturnsFalse_InvalidPassword()
    {
        // Arrange
        string password = "password123";
        string salt = PasswordUtils.GenerateSalt();
        string hashedPassword = PasswordUtils.HashPassword("invalidpassword", salt);

        // Act
        var isValid = PasswordUtils.IsPasswordValid(password, hashedPassword, salt);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsPasswordValid_ReturnsFalse_EmptyPassword()
    {
        // Arrange
        string password = "";
        string salt = PasswordUtils.GenerateSalt();
        string hashedPassword = PasswordUtils.HashPassword(password, salt);

        // Act
        var isValid = PasswordUtils.IsPasswordValid(password, hashedPassword, salt);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsPasswordValid_ReturnsFalse_EmptyHashedPassword()
    {
        // Arrange
        string password = "password123";
        string salt = PasswordUtils.GenerateSalt();
        string hashedPassword = "";

        // Act
        var isValid = PasswordUtils.IsPasswordValid(password, hashedPassword, salt);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsPasswordValid_ReturnsFalse_EmptySalt()
    {
        // Arrange
        string password = "password123";
        string salt = "";
        string hashedPassword = PasswordUtils.HashPassword(password, salt);

        // Act
        var isValid = PasswordUtils.IsPasswordValid(password, hashedPassword, salt);

        // Assert
        Assert.That(isValid, Is.False);
    }
}

