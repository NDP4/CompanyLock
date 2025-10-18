using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CompanyLock.LocalAuth.Services;
using Serilog;

namespace CompanyLock.Agent.Services;

public class LogCleanupService : BackgroundService
{
    private readonly LocalAuthService _authService;
    private readonly Serilog.ILogger _logger;
    private readonly TimeSpan _cleanupInterval;
    private readonly int _retentionDays;
    
    public LogCleanupService(LocalAuthService authService, int retentionDays = 30)
    {
        _authService = authService;
        _logger = Core.Logging.LoggerFactory.GetLogger();
        _retentionDays = retentionDays;
        
        // Run cleanup every 24 hours
        _cleanupInterval = TimeSpan.FromHours(24);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("Log cleanup service started with {RetentionDays} days retention", _retentionDays);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCleanupAsync();
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.Information("Log cleanup service cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in log cleanup service");
                // Wait 1 hour before retrying if there's an error
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
    
    private async Task PerformCleanupAsync()
    {
        try
        {
            var currentTime = DateTime.Now;
            
            // Log current database status before cleanup
            var logCountBefore = await _authService.GetLogCountAsync();
            var dbSizeBefore = await _authService.GetDatabaseSizeAsync();
            
            _logger.Information("Starting log cleanup - Current logs: {LogCount}, DB size: {DbSize} bytes", 
                logCountBefore, dbSizeBefore);
            
            // Perform the cleanup
            var deletedCount = await _authService.CleanupOldLogsAsync(_retentionDays);
            
            if (deletedCount > 0)
            {
                // Log database status after cleanup
                var logCountAfter = await _authService.GetLogCountAsync();
                var dbSizeAfter = await _authService.GetDatabaseSizeAsync();
                var sizeSaved = dbSizeBefore - dbSizeAfter;
                
                _logger.Information("Log cleanup completed - Deleted: {DeletedCount} logs, " +
                    "Remaining: {RemainingCount} logs, Space saved: {SpaceSaved} bytes", 
                    deletedCount, logCountAfter, sizeSaved);
                
                // Audit the cleanup action
                await _authService.LogEventAsync("LOG_CLEANUP", null, "SYSTEM", 
                    $"Automatic cleanup removed {deletedCount} logs older than {_retentionDays} days. " +
                    $"Space saved: {sizeSaved} bytes.");
            }
            else
            {
                _logger.Debug("No old logs found for cleanup");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to perform log cleanup");
            
            // Log the error as an audit event
            try
            {
                await _authService.LogEventAsync("LOG_CLEANUP_ERROR", null, "SYSTEM", 
                    $"Automatic log cleanup failed: {ex.Message}");
            }
            catch
            {
                // Ignore audit logging errors during cleanup
            }
        }
    }
    
    /// <summary>
    /// Manually trigger cleanup (useful for testing)
    /// </summary>
    public async Task<int> TriggerCleanupAsync()
    {
        try
        {
            _logger.Information("Manual log cleanup triggered");
            await PerformCleanupAsync();
            
            var currentLogCount = await _authService.GetLogCountAsync();
            return (int)currentLogCount;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to trigger manual cleanup");
            return -1;
        }
    }
    
    /// <summary>
    /// Get current log statistics
    /// </summary>
    public async Task<LogStatistics> GetLogStatisticsAsync()
    {
        try
        {
            var totalLogs = await _authService.GetLogCountAsync();
            var dbSize = await _authService.GetDatabaseSizeAsync();
            var cutoffDate = DateTime.Now.AddDays(-_retentionDays);
            
            return new LogStatistics
            {
                TotalLogs = totalLogs,
                DatabaseSize = dbSize,
                RetentionDays = _retentionDays,
                CutoffDate = cutoffDate,
                NextCleanupTime = DateTime.Now.Add(_cleanupInterval)
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get log statistics");
            return new LogStatistics();
        }
    }
}

public class LogStatistics
{
    public long TotalLogs { get; set; }
    public long DatabaseSize { get; set; }
    public int RetentionDays { get; set; }
    public DateTime CutoffDate { get; set; }
    public DateTime NextCleanupTime { get; set; }
    
    public string FormattedDatabaseSize => FormatBytes(DatabaseSize);
    
    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
        return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
    }
}