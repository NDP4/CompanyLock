using System.ComponentModel.DataAnnotations;

namespace CompanyLock.Core.Models;

public class Employee
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    public string Salt { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? FullName { get; set; }
    
    [MaxLength(50)]
    public string? Department { get; set; }
    
    [MaxLength(20)]
    public string Role { get; set; } = "User";
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime? LastSyncAt { get; set; }
}

public class Device
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string DeviceUuid { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Hostname { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? ComputerName { get; set; }
    
    [MaxLength(100)]
    public string? WindowsVersion { get; set; }
    
    public DateTime RegisteredAt { get; set; } = DateTime.Now;
    
    public DateTime? LastSeenAt { get; set; }
}

public class Session
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string SessionUuid { get; set; } = string.Empty;
    
    public int EmployeeId { get; set; }
    
    public int DeviceId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime? ExpiredAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Employee? Employee { get; set; }
    public Device? Device { get; set; }
}

public class AuditEvent
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;
    
    public int? EmployeeId { get; set; }
    
    public int DeviceId { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.Now;
    
    [MaxLength(50)]
    public string? SessionId { get; set; }
    
    // Navigation properties
    public Employee? Employee { get; set; }
    public Device? Device { get; set; }
}