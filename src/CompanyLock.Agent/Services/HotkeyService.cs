using System.Runtime.InteropServices;
using System.Timers;
using Serilog;

namespace CompanyLock.Agent.Services;

public class HotkeyService
{
    private readonly Serilog.ILogger _logger;
    private bool _isRegistered;
    private System.Timers.Timer _hotkeyTimer;
    private DateTime _lastCheck = DateTime.Now;
    
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    
    // Virtual key codes
    private const int VK_CONTROL = 0x11;
    private const int VK_ALT = 0x12;
    private const int VK_L = 0x4C;
    
    public event EventHandler? HotkeyPressed;
    
    public HotkeyService()
    {
        _logger = Core.Logging.LoggerFactory.GetLogger();
        _hotkeyTimer = new System.Timers.Timer();
        _hotkeyTimer.Interval = 100; // Check every 100ms
        _hotkeyTimer.Elapsed += CheckHotkey;
        _hotkeyTimer.AutoReset = true;
    }
    
    public void Start()
    {
        if (_isRegistered)
        {
            _logger.Warning("Hotkey service already running, skipping start");
            return;
        }
            
        try
        {
            _hotkeyTimer.Start();
            _isRegistered = true;
            _logger.Information("Hotkey service started - Monitoring Ctrl+Alt+L every {IntervalMs}ms", _hotkeyTimer.Interval);
            Console.WriteLine("[HotkeyService] Started - monitoring Ctrl+Alt+L");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to start hotkey monitoring");
        }
    }
    
    private void CheckHotkey(object? sender, ElapsedEventArgs e)
    {
        try
        {
            // Check if Ctrl+Alt+L is pressed
            bool ctrlPressed = (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0;
            bool altPressed = (GetAsyncKeyState(VK_ALT) & 0x8000) != 0;
            bool lPressed = (GetAsyncKeyState(VK_L) & 0x8000) != 0;
            
            if (ctrlPressed && altPressed && lPressed)
            {
                // Prevent multiple triggers
                if ((DateTime.Now - _lastCheck).TotalMilliseconds > 1000)
                {
                    _lastCheck = DateTime.Now;
                    _logger.Information("Hotkey Ctrl+Alt+L detected - Triggering lock screen");
                    Console.WriteLine("[HotkeyService] Ctrl+Alt+L pressed - launching lock screen");
                    HotkeyPressed?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error checking hotkey");
            Console.WriteLine($"[HotkeyService] ERROR: {ex.Message}");
        }
    }
    
    public void Stop()
    {
        if (!_isRegistered)
            return;
            
        try
        {
            _hotkeyTimer.Stop();
            _isRegistered = false;
            _logger.Information("Hotkey monitoring stopped");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to stop hotkey service");
        }
    }
}