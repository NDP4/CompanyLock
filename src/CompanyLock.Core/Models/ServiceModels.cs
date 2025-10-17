namespace CompanyLock.Core.Models;

public class AuthRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DeviceUuid { get; set; } = string.Empty;
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string SessionUuid { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public Employee? Employee { get; set; }
}

public class LockRequest
{
    public string Reason { get; set; } = string.Empty;
    public string DeviceUuid { get; set; } = string.Empty;
    public string? SessionId { get; set; }
}

public class UnlockRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DeviceUuid { get; set; } = string.Empty;
}

public class EventRequest
{
    public string EventType { get; set; } = string.Empty;
    public string DeviceUuid { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public string? Description { get; set; }
    public string? SessionId { get; set; }
}