using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Ogur.Terraria.Manager.Infrastructure.Config;

public class AppSettings
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "OgurTerrariaManager",
        "config.json"
    );

    // Ogur.Hub settings
    public string ApiUrl { get; set; } = "https://api.ogur.dev";
    public string Username { get; set; } = "";
    public string HashedPassword { get; set; } = "";

    // SSH settings
    public string SshHost { get; set; } = "";
    public int SshPort { get; set; } = 22;
    public string SshUsername { get; set; } = "";
    public string SshPasswordHash { get; set; } = "";

    public string ContainerName { get; set; } = "terraria";

    public bool AutoConnect { get; set; } = false;



    // Window settings
    public int WindowWidth { get; set; } = 900;
    public int WindowHeight { get; set; } = 700;
    public bool AlwaysOnTop { get; set; } = false;

    // UI settings
    public int FontSize { get; set; } = 12;
    public bool ShowTimestamps { get; set; } = true;

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to load config: {ex.Message}");
        }

        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(ConfigPath, json);

            Console.WriteLine($"✅ Config saved to: {ConfigPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to save config: {ex.Message}");
        }
    }

    public void SetPassword(string password)
    {
        HashedPassword = HashPassword(password);
    }

    public string? GetPassword()
    {
        return string.IsNullOrEmpty(HashedPassword) ? null : UnhashPassword(HashedPassword);
    }

    public void SetSshPassword(string password)
    {
        SshPasswordHash = HashPassword(password);
    }

    public string? GetSshPassword()
    {
        return string.IsNullOrEmpty(SshPasswordHash) ? null : UnhashPassword(SshPasswordHash);
    }

    private static string HashPassword(string password)
    {
        var entropy = Encoding.UTF8.GetBytes("OgurTerrariaManager2025");
        var data = Encoding.UTF8.GetBytes(password);
        var encrypted = ProtectedData.Protect(data, entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    private static string UnhashPassword(string hash)
    {
        try
        {
            var entropy = Encoding.UTF8.GetBytes("OgurTerrariaManager2025");
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
