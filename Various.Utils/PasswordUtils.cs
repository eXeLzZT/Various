using System;
using System.Security.Cryptography;

namespace Various.Utils;

public class PasswordUtils
{
    public static string GenerateSalt(int numberSalt = 16)
    {
        var saltBytes = new byte[numberSalt];

        using (var numberGenerator = RandomNumberGenerator.Create())
        {
            numberGenerator.GetNonZeroBytes(saltBytes);
        }

        return Convert.ToBase64String(saltBytes);
    }

    public static string HashPassword(string password, string salt, int numberInterations = 12288, int numberHash = 32)
    {
        var saltBytes = Convert.FromBase64String(salt);

        using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, saltBytes, numberInterations, HashAlgorithmName.SHA512))
        {
            return Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(numberHash));
        }
    }

    public static bool IsPasswordValid(string password, string hashedPassword, string salt)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        if (string.IsNullOrEmpty(hashedPassword))
            return false;

        if (string.IsNullOrEmpty(salt))
            return false;

        return HashPassword(password, salt) == hashedPassword;
    }
}
