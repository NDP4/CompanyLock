using Konscious.Security.Cryptography;
using System.Text;
using System.Security.Cryptography;

namespace CompanyLock.LocalAuth.Services;

public static class PasswordHelper
{
    public static string GenerateSalt()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
    
    public static string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        
        using var argon2 = new Argon2id(passwordBytes)
        {
            Salt = saltBytes,
            DegreeOfParallelism = 2,
            Iterations = 3,
            MemorySize = 65536 // 64 MB
        };
        
        var hashBytes = argon2.GetBytes(32);
        return Convert.ToBase64String(hashBytes);
    }
    
    public static bool VerifyPassword(string password, string salt, string hash)
    {
        try
        {
            var computedHash = HashPassword(password, salt);
            return computedHash == hash;
        }
        catch
        {
            return false;
        }
    }
}