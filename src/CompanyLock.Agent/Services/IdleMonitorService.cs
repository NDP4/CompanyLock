using System.Runtime.InteropServices;
using System.Timers;
using Serilog;

namespace CompanyLock.Agent.Services;

public class IdleMonitorService
{
    private readonly Serilog.ILogger _logger;
    private readonly System.Timers.Timer _idleTimer;
    private readonly int _idleTimeoutMs;
    private DateTime _lastActivity;
    private DateTime _lastIdleDetection = DateTime.MinValue;
    private bool _isMonitoring;
    
    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
    
    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }
    
    public event EventHandler? IdleDetected;
    public event EventHandler? ActivityDetected;
    
    public IdleMonitorService()
    {
        _logger = Core.Logging.LoggerFactory.GetLogger();
        _idleTimeoutMs = 60000; // 1 minute (production setting)
        _idleTimer = new System.Timers.Timer();
        _idleTimer.Interval = 5000; // Check every 5 seconds
        _idleTimer.Elapsed += CheckIdleStatus;
        _idleTimer.AutoReset = true;
        _lastActivity = DateTime.Now;
    }
    
    public void Start()
    {
        if (_isMonitoring)
        {
            _logger.Warning("Idle monitor already running, skipping start");
            return;
        }
            
        try
        {
            _lastActivity = DateTime.Now;
            _idleTimer.Start();
            _isMonitoring = true;
            _logger.Information("Idle monitor started - Timeout: {TimeoutMs}ms, Check interval: {IntervalMs}ms", _idleTimeoutMs, _idleTimer.Interval);
            Console.WriteLine($"[IdleMonitor] Started - 1 minute timeout");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to start idle monitor");
        }
    }
    
    public void Stop()
    {
        if (!_isMonitoring)
            return;
            
        try
        {
            _idleTimer.Stop();
            _isMonitoring = false;
            _logger.Information("Idle monitor stopped");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to stop idle monitor");
        }
    }
    
    private void CheckIdleStatus(object? sender, ElapsedEventArgs e)
    {
        try
        {
            var idleTime = GetIdleTime();
            var currentTime = DateTime.Now;
            
            if (idleTime >= _idleTimeoutMs)
            {
                // User is idle - check if we should trigger idle detection
                var timeSinceLastDetection = currentTime - _lastIdleDetection;
                var timeSinceLastActivity = currentTime - _lastActivity;
                
                // Only trigger if:
                // 1. Enough time passed since last activity AND
                // 2. Haven't triggered idle detection recently (prevent multiple triggers)
                if (timeSinceLastActivity >= TimeSpan.FromMilliseconds(_idleTimeoutMs) && 
                    timeSinceLastDetection >= TimeSpan.FromMinutes(2)) // Minimum 2 minutes between idle triggers
                {
                    _logger.Information("Idle timeout detected - User inactive for {IdleTimeMs}ms", idleTime);
                    Console.WriteLine($"[IdleMonitor] Idle timeout triggered - launching lock screen");
                    IdleDetected?.Invoke(this, EventArgs.Empty);
                    _lastIdleDetection = currentTime; // Record this detection
                }
            }
            else
            {
                // User is active - reset activity time
                _lastActivity = currentTime;
                
                // If user was idle before and now active, reset idle detection timer
                if (_lastIdleDetection != DateTime.MinValue && 
                    (currentTime - _lastIdleDetection).TotalMinutes < 5)
                {
                    // Reset idle detection if user becomes active within 5 minutes of last detection
                    _lastIdleDetection = DateTime.MinValue;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error checking idle status");
            Console.WriteLine($"[IdleMonitor] ERROR: {ex.Message}");
        }
    }
    
    private uint GetIdleTime()
    {
        var lastInputInfo = new LASTINPUTINFO();
        lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
        
        if (GetLastInputInfo(ref lastInputInfo))
        {
            var tickCount = (uint)Environment.TickCount;
            return tickCount - lastInputInfo.dwTime;
        }
        
        return 0;
    }
    
    public void ResetIdleTimer()
    {
        _lastActivity = DateTime.Now;
    }
}