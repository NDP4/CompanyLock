using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Serilog;

namespace CompanyLock.UI.Services;

public class SecureWindowService
{
    private readonly ILogger _logger;
    private Window? _lockWindow;
    private IntPtr _windowHandle;
    private bool _isSecureMode = false;

    // Windows API constants
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;
    private const int WS_SYSMENU = 0x80000;
    private const int WS_MINIMIZEBOX = 0x20000;
    private const int WS_MAXIMIZEBOX = 0x10000;
    private const int WS_EX_TOOLWINDOW = 0x80;
    private const int WS_EX_APPWINDOW = 0x40000;
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    public SecureWindowService()
    {
        _logger = Core.Logging.LoggerFactory.GetLogger();
    }

    public void EnableSecureMode(Window lockWindow)
    {
        if (_isSecureMode) return;

        try
        {
            _lockWindow = lockWindow;
            _windowHandle = new WindowInteropHelper(lockWindow).Handle;

            // Make window topmost and remove system menu
            SetWindowProperties();

            // Hide taskbar and start button
            HideTaskbar();

            // Disable task manager access warning
            _logger.Warning("Security: Task Manager cannot be fully disabled - it requires administrative privileges");

            _isSecureMode = true;
            _logger.Information("Secure window mode enabled");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to enable secure window mode");
        }
    }

    public void DisableSecureMode()
    {
        if (!_isSecureMode) return;

        try
        {
            // Restore taskbar
            ShowTaskbar();

            _isSecureMode = false;
            _logger.Information("Secure window mode disabled");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to disable secure window mode");
        }
    }

    private void SetWindowProperties()
    {
        if (_windowHandle == IntPtr.Zero) return;

        // Remove window decorations and system menu
        int currentStyle = GetWindowLong(_windowHandle, GWL_STYLE);
        int newStyle = currentStyle & ~(WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
        SetWindowLong(_windowHandle, GWL_STYLE, newStyle);

        // Set extended window style
        int currentExStyle = GetWindowLong(_windowHandle, GWL_EXSTYLE);
        int newExStyle = currentExStyle | WS_EX_TOOLWINDOW;
        newExStyle = newExStyle & ~WS_EX_APPWINDOW;
        SetWindowLong(_windowHandle, GWL_EXSTYLE, newExStyle);

        // Make sure window stays on top
        if (_lockWindow != null)
        {
            _lockWindow.Topmost = true;
            _lockWindow.WindowState = WindowState.Maximized;
            _lockWindow.WindowStyle = WindowStyle.None;
            _lockWindow.ResizeMode = ResizeMode.NoResize;
            _lockWindow.ShowInTaskbar = false;
        }
    }

    private void HideTaskbar()
    {
        try
        {
            IntPtr taskbarHandle = FindWindow("Shell_TrayWnd", "");
            if (taskbarHandle != IntPtr.Zero)
            {
                ShowWindow(taskbarHandle, SW_HIDE);
                _logger.Debug("Taskbar hidden for security");
            }

            // Hide start button
            IntPtr startHandle = FindWindow("Button", "Start");
            if (startHandle != IntPtr.Zero)
            {
                ShowWindow(startHandle, SW_HIDE);
                _logger.Debug("Start button hidden for security");
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Could not hide taskbar - may require elevated privileges");
        }
    }

    private void ShowTaskbar()
    {
        try
        {
            IntPtr taskbarHandle = FindWindow("Shell_TrayWnd", "");
            if (taskbarHandle != IntPtr.Zero)
            {
                ShowWindow(taskbarHandle, SW_SHOW);
                _logger.Debug("Taskbar restored");
            }

            // Show start button
            IntPtr startHandle = FindWindow("Button", "Start");
            if (startHandle != IntPtr.Zero)
            {
                ShowWindow(startHandle, SW_SHOW);
                _logger.Debug("Start button restored");
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Could not restore taskbar");
        }
    }

    public bool IsSecureMode => _isSecureMode;

    // Windows API imports
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    // Window positioning constants
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;

    public void EnsureWindowStaysOnTop()
    {
        if (_windowHandle != IntPtr.Zero)
        {
            SetWindowPos(_windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }
    }
}