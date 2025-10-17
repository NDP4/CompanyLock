namespace CompanyLock.Core.Configuration;

public class AppConfig
{
    public string DatabasePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CompanyLock", "local.db");
    public string LogPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CompanyLock", "Logs");
    public int IdleTimeoutMinutes { get; set; } = 1;
    public string LockHotkey { get; set; } = "Ctrl+Alt+L";
    public bool AutoStartOnBoot { get; set; } = true;
    public string PipeName { get; set; } = "CompanyLockPipe";
    public int SessionTimeoutHours { get; set; } = 8;
    public bool EnableAuditLogging { get; set; } = true;
    public string? EmergencyUnlockCode { get; set; }
}

public static class ConfigurationManager
{
    private static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CompanyLock", "config.json");
    private static AppConfig? _config;
    
    public static AppConfig GetConfig()
    {
        if (_config != null)
            return _config;
            
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                _config = System.Text.Json.JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            else
            {
                _config = new AppConfig();
                SaveConfig(_config);
            }
        }
        catch
        {
            _config = new AppConfig();
        }
        
        return _config;
    }
    
    public static void SaveConfig(AppConfig config)
    {
        try
        {
            var directory = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(ConfigPath, json);
            _config = config;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save configuration: {ex.Message}", ex);
        }
    }
}