using Serilog;

namespace CompanyLock.Core.Logging;

public static class LoggerFactory
{
    private static ILogger? _logger;
    
    public static ILogger GetLogger()
    {
        if (_logger != null)
            return _logger;
            
        var config = Configuration.ConfigurationManager.GetConfig();
        
        _logger = new LoggerConfiguration()
            .WriteTo.File(
                Path.Combine(config.LogPath, "companylock-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();
            
        return _logger;
    }
    
    public static void CloseAndFlush()
    {
        if (_logger is IDisposable disposableLogger)
        {
            disposableLogger.Dispose();
        }
        _logger = null;
    }
}