using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Serilog;

namespace CompanyLock.UI.Services;

public class SecurityEnforcementService
{
    private readonly ILogger _logger;
    private LowLevelKeyboardProc? _proc;
    private IntPtr _hookId = IntPtr.Zero;
    private bool _isSecurityActive = false;

    // Windows API constants
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;
    
    // Virtual key codes for blocked keys
    private const int VK_TAB = 0x09;
    private const int VK_F4 = 0x73;
    private const int VK_LWIN = 0x5B;
    private const int VK_RWIN = 0x5C;
    private const int VK_ESCAPE = 0x1B;
    private const int VK_DELETE = 0x2E;

    public SecurityEnforcementService()
    {
        _logger = Core.Logging.LoggerFactory.GetLogger();
        _proc = HookCallback;
    }

    public void EnableSecurityMode()
    {
        if (_isSecurityActive) return;

        try
        {
            _hookId = SetHook(_proc);
            _isSecurityActive = true;
            
            _logger.Information("Security enforcement enabled - keyboard shortcuts blocked");
            Console.WriteLine("[Security] Lock screen security active - Alt+F4, Alt+Tab, Windows key blocked");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to enable security mode");
        }
    }

    public void DisableSecurityMode()
    {
        if (!_isSecurityActive) return;

        try
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
            
            _isSecurityActive = false;
            _logger.Information("Security enforcement disabled");
            Console.WriteLine("[Security] Lock screen security disabled");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to disable security mode");
        }
    }

    private IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule!)
        {
            return SetWindowsHookEx(
                WH_KEYBOARD_LL,
                proc,
                GetModuleHandle(curModule.ModuleName),
                0);
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                
                if (IsBlockedKey(vkCode, wParam))
                {
                    _logger.Debug("Blocked keyboard shortcut: VK_{VkCode}", vkCode);
                    return (IntPtr)1; // Block the key
                }
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private bool IsBlockedKey(int vkCode, IntPtr wParam)
    {
        // Check for Alt+F4 (Alt + F4)
        if (vkCode == VK_F4 && (GetKeyState(0x12) & 0x8000) != 0) // Alt key
        {
            _logger.Warning("Security: Blocked Alt+F4 attempt");
            return true;
        }

        // Check for Alt+Tab (Alt + Tab)
        if (vkCode == VK_TAB && (GetKeyState(0x12) & 0x8000) != 0) // Alt key
        {
            _logger.Warning("Security: Blocked Alt+Tab attempt");
            return true;
        }

        // Block Windows key
        if (vkCode == VK_LWIN || vkCode == VK_RWIN)
        {
            _logger.Warning("Security: Blocked Windows key attempt");
            return true;
        }

        // Block Ctrl+Alt+Del detection (we can't fully block it, but we can log attempts)
        if (vkCode == VK_DELETE && 
            (GetKeyState(0x11) & 0x8000) != 0 && // Ctrl
            (GetKeyState(0x12) & 0x8000) != 0)   // Alt
        {
            _logger.Warning("Security: Ctrl+Alt+Del detected - cannot block system-level shortcut");
            // Note: We cannot block Ctrl+Alt+Del as it's handled at a lower level by Windows
            return false;
        }

        // Block Alt+Esc (alternative task switcher)
        if (vkCode == VK_ESCAPE && (GetKeyState(0x12) & 0x8000) != 0) // Alt key
        {
            _logger.Warning("Security: Blocked Alt+Esc attempt");
            return true;
        }

        return false;
    }

    public bool IsSecurityActive => _isSecurityActive;

    // Windows API imports
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    // Cleanup on dispose
    public void Dispose()
    {
        DisableSecurityMode();
        GC.SuppressFinalize(this);
    }

    ~SecurityEnforcementService()
    {
        DisableSecurityMode();
    }
}