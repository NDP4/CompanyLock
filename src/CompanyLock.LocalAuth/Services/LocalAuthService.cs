using Microsoft.EntityFrameworkCore;
using CompanyLock.Core.Models;
using CompanyLock.Core.Security;
using CompanyLock.LocalAuth.Data;
using Serilog;

namespace CompanyLock.LocalAuth.Services;

public class LocalAuthService
{
    private readonly string _connectionString;
    private readonly ILogger _logger;
    
    public LocalAuthService(string connectionString)
    {
        _connectionString = FormatConnectionString(connectionString);
        _logger = Core.Logging.LoggerFactory.GetLogger();
        InitializeDatabase();
    }
    
    private static string FormatConnectionString(string input)
    {
        // If it's already a proper connection string, return as is
        if (input.Contains("Data Source="))
            return input;
            
        // Convert simple path to proper SQLite connection string
        return $"Data Source={input};Cache=Shared;Mode=ReadWriteCreate;";
    }
    
    private void InitializeDatabase()
    {
        try
        {
            using var context = new LocalDbContext(_connectionString);
            context.Database.EnsureCreated();
            
            // Create default admin user if no employees exist
            if (!context.Employees.Any())
            {
                CreateDefaultAdmin();
            }
            
            _logger.Information("Database initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize database");
            throw;
        }
    }
    
    private void CreateDefaultAdmin()
    {
        try
        {
            using var context = new LocalDbContext(_connectionString);
            
            var salt = PasswordHelper.GenerateSalt();
            var hash = PasswordHelper.HashPassword("admin123", salt);
            
            var admin = new Employee
            {
                Username = "admin",
                PasswordHash = hash,
                Salt = salt,
                FullName = "System Administrator",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            
            context.Employees.Add(admin);
            context.SaveChanges();
            
            _logger.Information("Default admin user created");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to create default admin user");
        }
    }
    
    public async Task<AuthResponse> AuthenticateAsync(AuthRequest request)
    {
        try
        {
            using var context = new LocalDbContext(_connectionString);
            
            var employee = await context.Employees
                .FirstOrDefaultAsync(e => e.Username == request.Username && e.IsActive);
                
            if (employee == null)
            {
                await LogEventAsync("login_failed", null, request.DeviceUuid, "User not found");
                return new AuthResponse 
                { 
                    Success = false, 
                    ErrorMessage = "Invalid credentials" 
                };
            }
            
            if (!PasswordHelper.VerifyPassword(request.Password, employee.Salt, employee.PasswordHash))
            {
                await LogEventAsync("login_failed", employee.Id, request.DeviceUuid, "Invalid password");
                return new AuthResponse 
                { 
                    Success = false, 
                    ErrorMessage = "Invalid credentials" 
                };
            }
            
            // Create session
            var sessionUuid = SecurityHelper.GenerateSessionUuid();
            var device = await GetOrCreateDeviceAsync(request.DeviceUuid);
            
            var session = new Session
            {
                SessionUuid = sessionUuid,
                EmployeeId = employee.Id,
                DeviceId = device.Id,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            
            context.Sessions.Add(session);
            await context.SaveChangesAsync();
            
            await LogEventAsync("login_success", employee.Id, request.DeviceUuid, "User authenticated successfully");
            
            return new AuthResponse
            {
                Success = true,
                SessionUuid = sessionUuid,
                Employee = employee
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Authentication failed for user {Username}", request.Username);
            return new AuthResponse 
            { 
                Success = false, 
                ErrorMessage = "Authentication failed" 
            };
        }
    }
    
    private async Task<Device> GetOrCreateDeviceAsync(string deviceUuid)
    {
        using var context = new LocalDbContext(_connectionString);
        
        var device = await context.Devices
            .FirstOrDefaultAsync(d => d.DeviceUuid == deviceUuid);
            
        if (device == null)
        {
            device = new Device
            {
                DeviceUuid = deviceUuid,
                Hostname = Environment.MachineName,
                ComputerName = Environment.MachineName,
                WindowsVersion = Environment.OSVersion.ToString(),
                RegisteredAt = DateTime.Now,
                LastSeenAt = DateTime.Now
            };
            
            context.Devices.Add(device);
            await context.SaveChangesAsync();
        }
        else
        {
            device.LastSeenAt = DateTime.Now;
            await context.SaveChangesAsync();
        }
        
        return device;
    }
    
    public async Task LogEventAsync(string eventType, int? employeeId, string deviceUuid, string? description = null)
    {
        try
        {
            using var context = new LocalDbContext(_connectionString);
            
            var device = await GetOrCreateDeviceAsync(deviceUuid);
            
            var auditEvent = new AuditEvent
            {
                EventType = eventType,
                EmployeeId = employeeId,
                DeviceId = device.Id,
                Description = description,
                Timestamp = DateTime.Now // Use local time instead of UTC
            };
            
            context.AuditEvents.Add(auditEvent);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to log event {EventType}", eventType);
        }
    }
    
    public async Task<bool> ValidateSessionAsync(string sessionUuid)
    {
        try
        {
            using var context = new LocalDbContext(_connectionString);
            
            var session = await context.Sessions
                .FirstOrDefaultAsync(s => s.SessionUuid == sessionUuid && s.IsActive);
                
            return session != null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to validate session {SessionUuid}", sessionUuid);
            return false;
        }
    }
    
    public async Task InvalidateSessionAsync(string sessionUuid)
    {
        try
        {
            using var context = new LocalDbContext(_connectionString);
            
            var session = await context.Sessions
                .FirstOrDefaultAsync(s => s.SessionUuid == sessionUuid);
                
            if (session != null)
            {
                session.IsActive = false;
                session.ExpiredAt = DateTime.Now;
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to invalidate session {SessionUuid}", sessionUuid);
        }
    }
    
    // Log Management Methods
    public async Task<int> ClearAllLogsAsync()
    {
        try
        {
            using var context = new LocalDbContext(_connectionString);
            var count = await context.AuditEvents.CountAsync();
            context.AuditEvents.RemoveRange(context.AuditEvents);
            await context.SaveChangesAsync();
            
            _logger.Information("Cleared all audit logs ({Count} records)", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to clear all logs");
            return 0;
        }
    }
    
    public async Task<int> DeleteLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            using var context = new LocalDbContext(_connectionString);
            var logsToDelete = context.AuditEvents
                .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate);
            
            var count = await logsToDelete.CountAsync();
            context.AuditEvents.RemoveRange(logsToDelete);
            await context.SaveChangesAsync();
            
            _logger.Information("Deleted {Count} audit logs from {StartDate} to {EndDate}", 
                count, startDate, endDate);
            return count;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to delete logs by date range");
            return 0;
        }
    }
    
    public async Task<int> DeleteLogsByEventTypeAsync(string eventType)
    {
        try
        {
            using var context = new LocalDbContext(_connectionString);
            var logsToDelete = context.AuditEvents
                .Where(e => e.EventType == eventType);
            
            var count = await logsToDelete.CountAsync();
            context.AuditEvents.RemoveRange(logsToDelete);
            await context.SaveChangesAsync();
            
            _logger.Information("Deleted {Count} audit logs of type {EventType}", count, eventType);
            return count;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to delete logs by event type");
            return 0;
        }
    }
    
    public async Task<int> DeleteLogsByUsernameAsync(string username)
    {
        try
        {
            using var context = new LocalDbContext(_connectionString);
            
            // Get employee by username first
            var employee = await context.Employees
                .FirstOrDefaultAsync(e => e.Username == username);
                
            if (employee == null)
            {
                _logger.Warning("Employee with username {Username} not found", username);
                return 0;
            }
            
            var logsToDelete = context.AuditEvents
                .Where(e => e.EmployeeId == employee.Id);
            
            var count = await logsToDelete.CountAsync();
            context.AuditEvents.RemoveRange(logsToDelete);
            await context.SaveChangesAsync();
            
            _logger.Information("Deleted {Count} audit logs for user {Username}", count, username);
            return count;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to delete logs by username");
            return 0;
        }
    }
    
    public async Task<int> CleanupOldLogsAsync(int retentionDays = 30)
    {
        try
        {
            using var context = new LocalDbContext(_connectionString);
            var cutoffDate = DateTime.Now.AddDays(-retentionDays);
            
            var oldLogs = context.AuditEvents
                .Where(e => e.Timestamp < cutoffDate);
            
            var count = await oldLogs.CountAsync();
            
            if (count > 0)
            {
                context.AuditEvents.RemoveRange(oldLogs);
                await context.SaveChangesAsync();
                
                _logger.Information("Cleaned up {Count} old audit logs (older than {Days} days)", 
                    count, retentionDays);
            }
            
            return count;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to cleanup old logs");
            return 0;
        }
    }
    
    public async Task<long> GetLogCountAsync()
    {
        try
        {
            using var context = new LocalDbContext(_connectionString);
            return await context.AuditEvents.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get log count");
            return 0;
        }
    }
    
    public async Task<long> GetDatabaseSizeAsync()
    {
        try
        {
            var dbPath = _connectionString.Split("Data Source=")[1].Split(";")[0];
            if (File.Exists(dbPath))
            {
                var fileInfo = new FileInfo(dbPath);
                return fileInfo.Length;
            }
            return 0;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get database size");
            return 0;
        }
    }
}