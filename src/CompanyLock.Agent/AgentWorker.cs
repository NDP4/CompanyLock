using Microsoft.Extensions.Hosting;
using CompanyLock.Core.Configuration;
using CompanyLock.LocalAuth.Services;
using CompanyLock.Agent.Services;
using Serilog;

namespace CompanyLock.Agent;

public class AgentWorker : BackgroundService
{
    private readonly Serilog.ILogger _logger;
    private readonly AppConfig _config;
    private readonly LocalAuthService _authService;
    private readonly SessionMonitorService _sessionMonitor;
    private readonly IdleMonitorService _idleMonitor;
    private readonly HotkeyService _hotkeyService;
    private readonly PipeServerService _pipeServer;
    private readonly LogCleanupService _logCleanup;
    private volatile bool _isLockScreenActive = false;
    private readonly object _lockStateLock = new object();
    
    public AgentWorker(
        AppConfig config,
        LocalAuthService authService,
        SessionMonitorService sessionMonitor,
        IdleMonitorService idleMonitor,
        HotkeyService hotkeyService,
        PipeServerService pipeServer)
    {
        _logger = Core.Logging.LoggerFactory.GetLogger();
        _config = config;
        _authService = authService;
        _sessionMonitor = sessionMonitor;
        _idleMonitor = idleMonitor;
        _hotkeyService = hotkeyService;
        _pipeServer = pipeServer;
        
        // Initialize log cleanup service with 30 days retention
        _logCleanup = new LogCleanupService(_authService, 30);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("CompanyLock Agent Service starting...");
        
        try
        {
            // Initialize device registration
            await InitializeDevice();
            
            // Subscribe to events before starting services
            _logger.Information("Subscribing to security monitoring events");
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;
            _idleMonitor.IdleDetected += OnIdleDetected;
            
            // Start all monitoring services
            _logger.Information("Starting security monitoring services");
            _sessionMonitor.Start();
            _idleMonitor.Start();
            _hotkeyService.Start();
            _pipeServer.Start();
            
            // Start log cleanup service
            _logger.Information("Starting automatic log cleanup service (30 days retention)");
            _ = Task.Run(() => _logCleanup.StartAsync(stoppingToken), stoppingToken);
            
            Console.WriteLine("[CompanyLock] Security monitoring active - Hotkey: Ctrl+Alt+L | Idle timeout: 1 minute");
            Console.WriteLine("[CompanyLock] Automatic log cleanup enabled - 30 days retention");
            
            _logger.Information("CompanyLock Agent Service started successfully");
            
            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in AgentWorker");
            throw;
        }
    }
    
    private async Task InitializeDevice()
    {
        try
        {
            var deviceUuid = GetOrCreateDeviceUuid();
            await _authService.LogEventAsync("service_start", null, deviceUuid, "Agent service started");
            _logger.Information("Device initialized with UUID: {DeviceUuid}", deviceUuid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize device");
        }
    }
    
    private string GetOrCreateDeviceUuid()
    {
        try
        {
            // Try to read existing UUID from registry or config
            var existingUuid = GetStoredDeviceUuid();
            if (!string.IsNullOrEmpty(existingUuid))
            {
                return existingUuid;
            }
            
            // Generate new UUID
            var newUuid = Core.Security.SecurityHelper.GenerateDeviceUuid();
            StoreDeviceUuid(newUuid);
            
            return newUuid;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get or create device UUID");
            return Core.Security.SecurityHelper.GenerateDeviceUuid();
        }
    }
    
    private string? GetStoredDeviceUuid()
    {
        try
        {
            // For now, store in a simple file
            var uuidFile = Path.Combine(Path.GetDirectoryName(_config.DatabasePath) ?? "", "device.uuid");
            if (File.Exists(uuidFile))
            {
                var encryptedUuid = File.ReadAllText(uuidFile);
                return Core.Security.DpapiHelper.UnprotectData(encryptedUuid);
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to read stored device UUID");
        }
        
        return null;
    }
    
    private void StoreDeviceUuid(string uuid)
    {
        try
        {
            var uuidFile = Path.Combine(Path.GetDirectoryName(_config.DatabasePath) ?? "", "device.uuid");
            var encryptedUuid = Core.Security.DpapiHelper.ProtectData(uuid);
            File.WriteAllText(uuidFile, encryptedUuid);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to store device UUID");
        }
    }
    
    private async void OnHotkeyPressed(object? sender, EventArgs e)
    {
        try
        {
            lock (_lockStateLock)
            {
                if (_isLockScreenActive)
                {
                    _logger.Information("Hotkey pressed but lock screen already active - ignoring");
                    Console.WriteLine("[CompanyLock] Hotkey ignored - lock screen already active");
                    return;
                }
                
                _isLockScreenActive = true;
            }
            
            _logger.Information("Hotkey triggered - Launching lock screen");
            Console.WriteLine("[CompanyLock] Hotkey pressed - launching lock screen");
            await TriggerLockScreen("hotkey_triggered");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling hotkey press");
            Console.WriteLine($"[CompanyLock] ERROR handling hotkey: {ex.Message}");
            
            // Reset state on error
            lock (_lockStateLock)
            {
                _isLockScreenActive = false;
            }
        }
    }
    
    private async void OnIdleDetected(object? sender, EventArgs e)
    {
        try
        {
            lock (_lockStateLock)
            {
                if (_isLockScreenActive)
                {
                    _logger.Information("Idle timeout detected but lock screen already active - ignoring");
                    Console.WriteLine("[CompanyLock] Idle timeout ignored - lock screen already active");
                    return;
                }
                
                _isLockScreenActive = true;
            }
            
            _logger.Information("Idle timeout reached - Launching lock screen");
            Console.WriteLine("[CompanyLock] Idle timeout - launching lock screen");
            await TriggerLockScreen("idle_timeout");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling idle detection");
            Console.WriteLine($"[CompanyLock] ERROR handling idle: {ex.Message}");
            
            // Reset state on error
            lock (_lockStateLock)
            {
                _isLockScreenActive = false;
            }
        }
    }
    
    private async Task TriggerLockScreen(string reason)
    {
        try
        {
            var deviceUuid = GetOrCreateDeviceUuid();
            await _authService.LogEventAsync("lock_triggered", null, deviceUuid, $"Screen locked: {reason}");
            
            // Check if lock screen process is already running
            var existingProcesses = System.Diagnostics.Process.GetProcessesByName("CompanyLock.UI");
            if (existingProcesses.Length > 0)
            {
                _logger.Information("Lock screen already running, bringing to front");
                Console.WriteLine("[CompanyLock] Lock screen already active");
                
                // Reset state since lock screen is already active
                lock (_lockStateLock)
                {
                    _isLockScreenActive = false;
                }
                return;
            }
            
            // Launch the UI application to show lock screen
            var uiPath = Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\CompanyLock.UI\\bin\\Debug\\net8.0-windows\\CompanyLock.UI.exe");
            if (File.Exists(uiPath))
            {
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = uiPath,
                    UseShellExecute = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized
                });
                
                if (process != null)
                {
                    _logger.Information("Lock screen UI launched: {Reason}", reason);
                    Console.WriteLine($"[CompanyLock] Lock screen launched - PID: {process.Id}");
                    
                    // Monitor process in background to reset state when it exits
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await process.WaitForExitAsync();
                            lock (_lockStateLock)
                            {
                                _isLockScreenActive = false;
                            }
                            Console.WriteLine("[CompanyLock] Lock screen closed - monitoring resumed");
                            _logger.Information("Lock screen process exited - monitoring resumed");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Error monitoring lock screen process");
                            lock (_lockStateLock)
                            {
                                _isLockScreenActive = false;
                            }
                        }
                    });
                }
            }
            else
            {
                _logger.Warning("Lock screen UI not found at: {Path}", uiPath);
                Console.WriteLine($"[CompanyLock] ERROR: Lock screen UI not found");
                
                // Reset state on error
                lock (_lockStateLock)
                {
                    _isLockScreenActive = false;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to trigger lock screen for reason: {Reason}", reason);
            Console.WriteLine($"[CompanyLock] ERROR launching lock screen: {ex.Message}");
            
            // Reset state on error
            lock (_lockStateLock)
            {
                _isLockScreenActive = false;
            }
        }
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Information("CompanyLock Agent Service stopping...");
        
        _sessionMonitor?.Stop();
        _idleMonitor?.Stop();
        _hotkeyService?.Stop();
        _pipeServer?.Stop();
        
        // Stop log cleanup service
        if (_logCleanup != null)
        {
            await _logCleanup.StopAsync(cancellationToken);
            _logger.Information("Log cleanup service stopped");
        }
        
        await base.StopAsync(cancellationToken);
        
        _logger.Information("CompanyLock Agent Service stopped");
    }
}