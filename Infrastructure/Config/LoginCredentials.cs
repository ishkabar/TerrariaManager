using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Ogur.Terraria.Manager.Infrastructure.Config;

public class LoginCredentials
{
    private static readonly string CredentialsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "OgurTerrariaManager",
        "credentials.json"
    );

    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public bool RememberMe { get; set; } = false;

    public static LoginCredentials Load()
    {
        try
        {
            if (File.Exists(CredentialsPath))
            {
                var json = File.ReadAllText(CredentialsPath);
                return JsonSerializer.Deserialize<LoginCredentials>(json) ?? new LoginCredentials();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load credentials: {ex.Message}");
        }

        return new LoginCredentials();
    }

    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(CredentialsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(CredentialsPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save credentials: {ex.Message}");
        }
    }

    public void Clear()
    {
        Username = "";
        PasswordHash = "";
        RememberMe = false;

        try
        {
            if (File.Exists(CredentialsPath))
                File.Delete(CredentialsPath);
        }
        catch { }
    }

    public void SetPassword(string password)
    {
        PasswordHash = HashPassword(password);
    }

    public string? GetPassword()
    {
        return string.IsNullOrEmpty(PasswordHash) ? null : UnhashPassword(PasswordHash);
    }

    private static string HashPassword(string password)
    {
        var entropy = Encoding.UTF8.GetBytes("OgurLoginCredentials2025");
        var data = Encoding.UTF8.GetBytes(password);
        var encrypted = ProtectedData.Protect(data, entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    private static string UnhashPassword(string hash)
    {
        try
        {
            var entropy = Encoding.UTF8.GetBytes("OgurLoginCredentials2025");
            var encrypted = Convert.FromBase64String(hash);
            var decrypted = ProtectedData.Unprotect(encrypted, entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return "";
        }
    }
}