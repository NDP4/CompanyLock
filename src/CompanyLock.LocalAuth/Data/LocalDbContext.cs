using Microsoft.EntityFrameworkCore;
using CompanyLock.Core.Models;

namespace CompanyLock.LocalAuth.Data;

public class LocalDbContext : DbContext
{
    private readonly string _connectionString;
    
    public LocalDbContext(string connectionString)
    {
        _connectionString = connectionString;
        EnsureDatabaseDirectoryExists();
    }
    
    private void EnsureDatabaseDirectoryExists()
    {
        try
        {
            // Extract directory path from connection string
            var dbPath = ExtractDatabasePathFromConnectionString(_connectionString);
            if (!string.IsNullOrEmpty(dbPath))
            {
                var directory = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    
                    // Ensure directory is writable
                    var testFile = Path.Combine(directory, "test.tmp");
                    File.WriteAllText(testFile, "test");
                    File.Delete(testFile);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Cannot create or access database directory: {ex.Message}", ex);
        }
    }
    
    private string ExtractDatabasePathFromConnectionString(string connectionString)
    {
        // Handle both "Data Source=path" and simple path formats
        if (connectionString.Contains("Data Source="))
        {
            var parts = connectionString.Split(';');
            var dataSourcePart = parts.FirstOrDefault(p => p.Trim().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase));
            if (dataSourcePart != null)
            {
                return dataSourcePart.Substring("Data Source=".Length).Trim();
            }
        }
        return connectionString; // Assume it's just the path
    }
    
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<AuditEvent> AuditEvents { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionString);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Role).HasDefaultValue("User");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
        
        // Device configuration
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasIndex(e => e.DeviceUuid).IsUnique();
        });
        
        // Session configuration
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasIndex(e => e.SessionUuid).IsUnique();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Device)
                .WithMany()
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // AuditEvent configuration
        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Device)
                .WithMany()
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}