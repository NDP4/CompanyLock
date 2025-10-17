using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using CompanyLock.Core.Models;
using CompanyLock.LocalAuth.Services;
using Serilog;

namespace CompanyLock.Agent.Services;

public class PipeServerService
{
    private readonly Serilog.ILogger _logger;
    private readonly LocalAuthService _authService;
    private readonly string _pipeName = "CompanyLockPipe";
    private bool _isRunning;
    private CancellationTokenSource? _cancellationTokenSource;
    
    public PipeServerService(LocalAuthService authService)
    {
        _logger = Core.Logging.LoggerFactory.GetLogger();
        _authService = authService;
    }
    
    public void Start()
    {
        if (_isRunning)
            return;
            
        _cancellationTokenSource = new CancellationTokenSource();
        _isRunning = true;
        
        Task.Run(() => RunServerAsync(_cancellationTokenSource.Token));
        _logger.Information("Named pipe server started on pipe: {PipeName}", _pipeName);
    }
    
    public void Stop()
    {
        if (!_isRunning)
            return;
            
        _cancellationTokenSource?.Cancel();
        _isRunning = false;
        _logger.Information("Named pipe server stopped");
    }
    
    private async Task RunServerAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var pipeServer = new NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Message);
                
                _logger.Debug("Waiting for client connection...");
                await pipeServer.WaitForConnectionAsync(cancellationToken);
                _logger.Debug("Client connected to pipe");
                
                await HandleClientAsync(pipeServer, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in pipe server");
                await Task.Delay(1000, cancellationToken);
            }
        }
    }
    
    private async Task HandleClientAsync(NamedPipeServerStream pipeServer, CancellationToken cancellationToken)
    {
        try
        {
            var buffer = new byte[4096];
            var totalBytes = 0;
            var messageBytes = new List<byte>();
            
            while (true)
            {
                var bytesRead = await pipeServer.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead == 0)
                    break;
                    
                messageBytes.AddRange(buffer.Take(bytesRead));
                totalBytes += bytesRead;
                
                if (!pipeServer.IsMessageComplete)
                    continue;
                    
                var message = Encoding.UTF8.GetString(messageBytes.ToArray());
                var response = await ProcessMessageAsync(message);
                
                var responseBytes = Encoding.UTF8.GetBytes(response);
                await pipeServer.WriteAsync(responseBytes, 0, responseBytes.Length, cancellationToken);
                await pipeServer.FlushAsync(cancellationToken);
                
                messageBytes.Clear();
                totalBytes = 0;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling pipe client");
        }
    }
    
    private async Task<string> ProcessMessageAsync(string message)
    {
        try
        {
            _logger.Debug("Processing message: {Message}", message);
            
            var request = JsonSerializer.Deserialize<PipeRequest>(message);
            if (request == null)
            {
                return JsonSerializer.Serialize(new PipeResponse 
                { 
                    Success = false, 
                    ErrorMessage = "Invalid request format" 
                });
            }
            
            return request.Action switch
            {
                "authenticate" => await HandleAuthenticateAsync(request),
                "log_event" => await HandleLogEventAsync(request),
                "validate_session" => await HandleValidateSessionAsync(request),
                "invalidate_session" => await HandleInvalidateSessionAsync(request),
                _ => JsonSerializer.Serialize(new PipeResponse 
                { 
                    Success = false, 
                    ErrorMessage = "Unknown action" 
                })
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error processing pipe message");
            return JsonSerializer.Serialize(new PipeResponse 
            { 
                Success = false, 
                ErrorMessage = "Internal error" 
            });
        }
    }
    
    private async Task<string> HandleAuthenticateAsync(PipeRequest request)
    {
        try
        {
            var authRequest = JsonSerializer.Deserialize<AuthRequest>(request.Data);
            if (authRequest == null)
            {
                return JsonSerializer.Serialize(new PipeResponse 
                { 
                    Success = false, 
                    ErrorMessage = "Invalid authentication request" 
                });
            }
            
            var authResponse = await _authService.AuthenticateAsync(authRequest);
            return JsonSerializer.Serialize(new PipeResponse 
            { 
                Success = authResponse.Success,
                Data = JsonSerializer.Serialize(authResponse),
                ErrorMessage = authResponse.ErrorMessage
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling authentication");
            return JsonSerializer.Serialize(new PipeResponse 
            { 
                Success = false, 
                ErrorMessage = "Authentication failed" 
            });
        }
    }
    
    private async Task<string> HandleLogEventAsync(PipeRequest request)
    {
        try
        {
            var eventRequest = JsonSerializer.Deserialize<EventRequest>(request.Data);
            if (eventRequest == null)
            {
                return JsonSerializer.Serialize(new PipeResponse 
                { 
                    Success = false, 
                    ErrorMessage = "Invalid event request" 
                });
            }
            
            await _authService.LogEventAsync(
                eventRequest.EventType,
                eventRequest.EmployeeId,
                eventRequest.DeviceUuid,
                eventRequest.Description);
                
            return JsonSerializer.Serialize(new PipeResponse { Success = true });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error logging event");
            return JsonSerializer.Serialize(new PipeResponse 
            { 
                Success = false, 
                ErrorMessage = "Event logging failed" 
            });
        }
    }
    
    private async Task<string> HandleValidateSessionAsync(PipeRequest request)
    {
        try
        {
            var sessionUuid = request.Data;
            var isValid = await _authService.ValidateSessionAsync(sessionUuid);
            
            return JsonSerializer.Serialize(new PipeResponse 
            { 
                Success = isValid,
                Data = isValid.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error validating session");
            return JsonSerializer.Serialize(new PipeResponse 
            { 
                Success = false, 
                ErrorMessage = "Session validation failed" 
            });
        }
    }
    
    private async Task<string> HandleInvalidateSessionAsync(PipeRequest request)
    {
        try
        {
            var sessionUuid = request.Data;
            await _authService.InvalidateSessionAsync(sessionUuid);
            
            return JsonSerializer.Serialize(new PipeResponse { Success = true });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error invalidating session");
            return JsonSerializer.Serialize(new PipeResponse 
            { 
                Success = false, 
                ErrorMessage = "Session invalidation failed" 
            });
        }
    }
    
    private class PipeRequest
    {
        public string Action { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }
    
    private class PipeResponse
    {
        public bool Success { get; set; }
        public string? Data { get; set; }
        public string? ErrorMessage { get; set; }
    }
}