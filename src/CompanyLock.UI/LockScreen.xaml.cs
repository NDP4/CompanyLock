using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CompanyLock.Core.Models;

namespace CompanyLock.UI;

public partial class LockScreen : Window
{
    private readonly DispatcherTimer _timeTimer;
    private readonly string _pipeName = "CompanyLockPipe";
    private string _deviceUuid = string.Empty;
    
    public LockScreen()
    {
        InitializeComponent();
        
        _timeTimer = new DispatcherTimer();
        _timeTimer.Interval = TimeSpan.FromSeconds(1);
        _timeTimer.Tick += UpdateTime;
        _timeTimer.Start();
        
        _deviceUuid = GetDeviceUuid();
        
        // Focus on username field
        Loaded += (s, e) => UsernameTextBox.Focus();
        
        // Prevent Alt+Tab and other escape methods
        PreviewKeyDown += LockScreen_PreviewKeyDown;
    }
    
    private void UpdateTime(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        TimeDisplay.Text = now.ToString("HH:mm:ss");
        DateDisplay.Text = now.ToString("dddd, dd MMMM yyyy");
    }
    
    private string GetDeviceUuid()
    {
        try
        {
            // Get device UUID from the agent or generate a temporary one
            return Environment.MachineName + "_" + Environment.UserName;
        }
        catch
        {
            return "TEMP_DEVICE_UUID";
        }
    }
    
    private void LockScreen_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Block common escape key combinations
        if (e.Key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            e.Handled = true;
        }
        
        if (e.Key == Key.Escape ||
            e.Key == Key.F4 && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt ||
            e.Key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            e.Handled = true;
        }
    }
    
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AttemptUnlock();
        }
    }
    
    private void UsernameTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            PasswordBox.Focus();
        }
        
        HideError();
    }
    
    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AttemptUnlock();
        }
        
        HideError();
    }
    
    private void UnlockButton_Click(object sender, RoutedEventArgs e)
    {
        AttemptUnlock();
    }
    
    private async void AttemptUnlock()
    {
        var username = UsernameTextBox.Text.Trim();
        var password = PasswordBox.Password;
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Username dan password harus diisi");
            return;
        }
        
        SetLoading(true);
        
        try
        {
            var authRequest = new AuthRequest
            {
                Username = username,
                Password = password,
                DeviceUuid = _deviceUuid
            };
            
            var result = await AuthenticateWithAgentAsync(authRequest);
            
            if (result.Success)
            {
                // Authentication successful - close the lock screen
                this.Close();
            }
            else
            {
                ShowError(result.ErrorMessage ?? "Autentikasi gagal");
                PasswordBox.Clear();
                PasswordBox.Focus();
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error koneksi: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }
    
    private async Task<AuthResponse> AuthenticateWithAgentAsync(AuthRequest request)
    {
        try
        {
            using var pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut);
            await pipeClient.ConnectAsync(5000); // 5 second timeout
            
            var pipeRequest = new
            {
                Action = "authenticate",
                Data = JsonSerializer.Serialize(request)
            };
            
            var requestJson = JsonSerializer.Serialize(pipeRequest);
            var requestBytes = Encoding.UTF8.GetBytes(requestJson);
            
            await pipeClient.WriteAsync(requestBytes, 0, requestBytes.Length);
            await pipeClient.FlushAsync();
            
            var responseBuffer = new byte[4096];
            var bytesRead = await pipeClient.ReadAsync(responseBuffer, 0, responseBuffer.Length);
            var responseJson = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
            
            var pipeResponse = JsonSerializer.Deserialize<PipeResponse>(responseJson);
            if (pipeResponse == null || !pipeResponse.Success)
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    ErrorMessage = pipeResponse?.ErrorMessage ?? "Komunikasi dengan service gagal" 
                };
            }
            
            if (!string.IsNullOrEmpty(pipeResponse.Data))
            {
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(pipeResponse.Data);
                return authResponse ?? new AuthResponse { Success = false, ErrorMessage = "Invalid response format" };
            }
            
            return new AuthResponse { Success = true };
        }
        catch (TimeoutException)
        {
            return new AuthResponse { Success = false, ErrorMessage = "Service tidak merespons" };
        }
        catch (Exception ex)
        {
            return new AuthResponse { Success = false, ErrorMessage = $"Error: {ex.Message}" };
        }
    }
    
    private void ShowError(string message)
    {
        ErrorMessage.Text = message;
        ErrorPanel.Visibility = Visibility.Visible;
    }
    
    private void HideError()
    {
        ErrorPanel.Visibility = Visibility.Collapsed;
    }
    
    private void SetLoading(bool isLoading)
    {
        UnlockButton.IsEnabled = !isLoading;
        UsernameTextBox.IsEnabled = !isLoading;
        PasswordBox.IsEnabled = !isLoading;
        
        if (isLoading)
        {
            StatusMessage.Text = "Verifying credentials...";
            UnlockButton.Content = "🔍 VERIFYING...";
        }
        else
        {
            StatusMessage.Text = "System is secured. Authentication required.";
            UnlockButton.Content = "🔓 UNLOCK WORKSTATION";
        }
    }
    
    private class PipeResponse
    {
        public bool Success { get; set; }
        public string? Data { get; set; }
        public string? ErrorMessage { get; set; }
    }
}