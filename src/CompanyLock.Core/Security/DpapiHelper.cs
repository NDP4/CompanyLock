using System.Security.Cryptography;
using System.Text;
using System.Runtime.Versioning;

namespace CompanyLock.Core.Security;

[SupportedOSPlatform("windows")]
public static class DpapiHelper
{
    [SupportedOSPlatform("windows")]
    public static string ProtectData(string data, bool userScope = false)
    {
        if (string.IsNullOrEmpty(data))
            throw new ArgumentException("Data cannot be null or empty", nameof(data));
            
        try
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var protectedBytes = System.Security.Cryptography.ProtectedData.Protect(
                bytes, 
                null, 
                userScope ? System.Security.Cryptography.DataProtectionScope.CurrentUser : System.Security.Cryptography.DataProtectionScope.LocalMachine
            );
            return Convert.ToBase64String(protectedBytes);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to protect data: {ex.Message}", ex);
        }
    }
    
    [SupportedOSPlatform("windows")]
    public static string UnprotectData(string protectedData, bool userScope = false)
    {
        if (string.IsNullOrEmpty(protectedData))
            throw new ArgumentException("Protected data cannot be null or empty", nameof(protectedData));
            
        try
        {
            var protectedBytes = Convert.FromBase64String(protectedData);
            var bytes = System.Security.Cryptography.ProtectedData.Unprotect(
                protectedBytes, 
                null, 
                userScope ? System.Security.Cryptography.DataProtectionScope.CurrentUser : System.Security.Cryptography.DataProtectionScope.LocalMachine
            );
            return Encoding.UTF8.GetString(bytes);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to unprotect data: {ex.Message}", ex);
        }
    }
}

public static class SecurityHelper
{
    public static string GenerateRandomString(int length = 32)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
    public static string GenerateDeviceUuid()
    {
        return Guid.NewGuid().ToString();
    }
    
    public static string GenerateSessionUuid()
    {
        return Guid.NewGuid().ToString();
    }
}