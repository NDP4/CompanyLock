using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;
using Serilog;

namespace CompanyLock.Agent.Services;

public class SessionMonitorService
{
    private readonly Serilog.ILogger _logger;
    private bool _isMonitoring;
    
    public SessionMonitorService()
    {
        _logger = Core.Logging.LoggerFactory.GetLogger();
    }
    
    public void Start()
    {
        if (_isMonitoring)
            return;
            
        try
        {
            SystemEvents.SessionSwitch += OnSessionSwitch;
            _isMonitoring = true;
            _logger.Information("Session monitor started");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to start session monitor");
        }
    }
    
    public void Stop()
    {
        if (!_isMonitoring)
            return;
            
        try
        {
            SystemEvents.SessionSwitch -= OnSessionSwitch;
            _isMonitoring = false;
            _logger.Information("Session monitor stopped");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to stop session monitor");
        }
    }
    
    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        try
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLogon:
                    _logger.Information("User session logon detected");
                    SpawnLockUI();
                    break;
                    
                case SessionSwitchReason.SessionLogoff:
                    _logger.Information("User session logoff detected");
                    break;
                    
                case SessionSwitchReason.SessionLock:
                    _logger.Information("User session lock detected");
                    break;
                    
                case SessionSwitchReason.SessionUnlock:
                    _logger.Information("User session unlock detected");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling session switch event");
        }
    }
    
    private void SpawnLockUI()
    {
        try
        {
            // Get the path to the UI executable - try multiple locations
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] possiblePaths = {
                @"C:\Program Files\CompanyLock\UI\CompanyLock.UI.exe", // Production: Absolute path for Ghost Spectre compatibility
                Path.Combine(currentDir, "..", "UI", "CompanyLock.UI.exe"), // Production: Relative path
                Path.Combine(currentDir, "CompanyLock.UI.exe"), // Same folder
                Path.Combine(currentDir, "..\\..\\..\\..\\CompanyLock.UI\\bin\\Debug\\net8.0-windows\\CompanyLock.UI.exe") // Development
            };
            
            string? uiPath = null;
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    uiPath = path;
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(uiPath))
            {
                _logger.Error("UI executable not found. Tried paths: {Paths}", string.Join(", ", possiblePaths));
                return;
            }
            
            // Start the UI process in the current session
            var startInfo = new ProcessStartInfo
            {
                FileName = uiPath,
                UseShellExecute = true,
                CreateNoWindow = false
            };
            
            var process = Process.Start(startInfo);
            if (process != null)
            {
                _logger.Information("Lock UI spawned successfully, Process ID: {ProcessId}", 
                    process.Id);
            }
            else
            {
                _logger.Warning("Failed to spawn Lock UI");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to spawn Lock UI");
        }
    }
}