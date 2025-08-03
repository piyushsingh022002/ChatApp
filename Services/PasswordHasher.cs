using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace ChatApp.Api.Services;

public class PasswordHasher
{
    public string Hash(string password)
    {
        byte[] salt = new byte[128 / 8];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password, salt,
            KeyDerivationPrf.HMACSHA256,
            10000, 32));

        return $"{Convert.ToBase64String(salt)}.{hashed}";
    }

    public bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);

        var attempt = KeyDerivation.Pbkdf2(
            password, salt,
            KeyDerivationPrf.HMACSHA256,
            10000, 32);

        return hash.SequenceEqual(attempt);
    }
}
