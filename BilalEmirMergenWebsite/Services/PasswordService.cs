using System.Security.Cryptography;

namespace BilalEmirMergenWebsite.Services;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string storedHash);
}

public sealed class PasswordService : IPasswordService
{
    private const int Iterations = 120_000;
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return $"pbkdf2-sha256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string storedHash)
    {
        if (storedHash.StartsWith("pbkdf2-sha256$", StringComparison.Ordinal))
        {
            var parts = storedHash.Split('$');
            if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations)) return false;
            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var expected = Convert.FromBase64String(parts[3]);
                var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException) { return false; }
        }

        // Backward compatibility for the original SHA-256 admin hashes.
        var legacy = Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password)));
        return CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(legacy), System.Text.Encoding.UTF8.GetBytes(storedHash));
    }
}
