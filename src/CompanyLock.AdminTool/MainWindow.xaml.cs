using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CompanyLock.Core.Configuration;
using CompanyLock.Core.Models;
using CompanyLock.LocalAuth.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using CsvHelper;
using System.Globalization;
using CompanyLock.LocalAuth.Data;

namespace CompanyLock.AdminTool;

public partial class MainWindow : Window
{
    private readonly LocalAuthService _authService;
    private readonly ObservableCollection<Employee> _employees;
    private readonly ObservableCollection<AuditEvent> _auditEvents;
    
    public MainWindow()
    {
        InitializeComponent();
        
        var config = ConfigurationManager.GetConfig();
        _authService = new LocalAuthService($"Data Source={config.DatabasePath}");
        
        _employees = new ObservableCollection<Employee>();
        _auditEvents = new ObservableCollection<AuditEvent>();
        
        EmployeeDataGrid.ItemsSource = _employees;
        AuditLogsDataGrid.ItemsSource = _auditEvents;
        
        LoadEmployees();
        LoadAuditLogs();
    }
    
    private async void LoadEmployees()
    {
        try
        {
            StatusLabel.Text = "Loading employees...";
            
            var config = ConfigurationManager.GetConfig();
            using var context = new LocalDbContext($"Data Source={config.DatabasePath}");
            
            var employees = await context.Employees.ToListAsync();
            
            _employees.Clear();
            foreach (var employee in employees)
            {
                _employees.Add(employee);
            }
            
            StatusLabel.Text = $"Loaded {employees.Count} employees";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading employees: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusLabel.Text = "Error loading employees";
        }
    }
    
    private async void LoadAuditLogs()
    {
        try
        {
            var config = ConfigurationManager.GetConfig();
            using var context = new LocalDbContext($"Data Source={config.DatabasePath}");
            
            var logs = await context.AuditEvents
                .Include(e => e.Employee)
                .Include(e => e.Device)
                .OrderByDescending(e => e.Timestamp)
                .Take(1000) // Limit to last 1000 entries
                .ToListAsync();
            
            _auditEvents.Clear();
            foreach (var log in logs)
            {
                _auditEvents.Add(log);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading audit logs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void ImportCsvButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Select CSV file to import"
        };
        
        if (openFileDialog.ShowDialog() == true)
        {
            ImportCsvFile(openFileDialog.FileName);
        }
    }
    
    private async void ImportCsvFile(string filePath)
    {
        try
        {
            StatusLabel.Text = "Importing CSV...";
            
            var config = ConfigurationManager.GetConfig();
            using var context = new LocalDbContext($"Data Source={config.DatabasePath}");
            
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            var records = csv.GetRecords<EmployeeCsvRecord>().ToList();
            var importCount = 0;
            
            foreach (var record in records)
            {
                // Check if employee already exists
                var existingEmployee = await context.Employees
                    .FirstOrDefaultAsync(e => e.Username == record.Username);
                
                if (existingEmployee != null)
                {
                    // Update existing employee
                    existingEmployee.FullName = record.FullName;
                    existingEmployee.Department = record.Department;
                    existingEmployee.Role = record.Role ?? "User";
                    existingEmployee.IsActive = record.IsActive;
                    existingEmployee.LastSyncAt = DateTime.Now;
                    
                    // Update password if provided
                    if (!string.IsNullOrEmpty(record.Password))
                    {
                        var salt = PasswordHelper.GenerateSalt();
                        var hash = PasswordHelper.HashPassword(record.Password, salt);
                        existingEmployee.Salt = salt;
                        existingEmployee.PasswordHash = hash;
                    }
                }
                else
                {
                    // Create new employee
                    var salt = PasswordHelper.GenerateSalt();
                    var hash = PasswordHelper.HashPassword(record.Password ?? "password123", salt);
                    
                    var employee = new Employee
                    {
                        Username = record.Username,
                        PasswordHash = hash,
                        Salt = salt,
                        FullName = record.FullName,
                        Department = record.Department,
                        Role = record.Role ?? "User",
                        IsActive = record.IsActive,
                        CreatedAt = DateTime.Now,
                        LastSyncAt = DateTime.Now
                    };
                    
                    context.Employees.Add(employee);
                }
                
                importCount++;
            }
            
            await context.SaveChangesAsync();
            LoadEmployees();
            
            StatusLabel.Text = $"Imported {importCount} employees successfully";
            MessageBox.Show($"Successfully imported {importCount} employees", "Import Complete", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error importing CSV: {ex.Message}", "Import Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            StatusLabel.Text = "Import failed";
        }
    }
    
    private void ExportCsvButton_Click(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Export employees to CSV",
            FileName = $"employees_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };
        
        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                StatusLabel.Text = "Exporting CSV...";
                
                using var writer = new StreamWriter(saveFileDialog.FileName);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                
                var exportRecords = _employees.Select(e => new EmployeeCsvRecord
                {
                    Username = e.Username,
                    FullName = e.FullName ?? "",
                    Department = e.Department ?? "",
                    Role = e.Role,
                    IsActive = e.IsActive,
                    Password = "" // Don't export passwords
                }).ToList();
                
                csv.WriteRecords(exportRecords);
                
                StatusLabel.Text = $"Exported {exportRecords.Count} employees";
                MessageBox.Show($"Successfully exported {exportRecords.Count} employees", "Export Complete", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting CSV: {ex.Message}", "Export Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusLabel.Text = "Export failed";
            }
        }
    }
    
    private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new EmployeeDialog();
        if (dialog.ShowDialog() == true)
        {
            LoadEmployees();
        }
    }
    
    private void EditEmployeeButton_Click(object sender, RoutedEventArgs e)
    {
        if (EmployeeDataGrid.SelectedItem is Employee selectedEmployee)
        {
            var dialog = new EmployeeDialog(selectedEmployee);
            if (dialog.ShowDialog() == true)
            {
                LoadEmployees();
            }
        }
        else
        {
            MessageBox.Show("Please select an employee to edit.", "No Selection", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
    
    private async void DeleteEmployeeButton_Click(object sender, RoutedEventArgs e)
    {
        if (EmployeeDataGrid.SelectedItem is Employee selectedEmployee)
        {
            var result = MessageBox.Show($"Are you sure you want to delete employee '{selectedEmployee.Username}'?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var config = ConfigurationManager.GetConfig();
                    using var context = new LocalDbContext($"Data Source={config.DatabasePath}");
                    
                    var employee = await context.Employees.FindAsync(selectedEmployee.Id);
                    if (employee != null)
                    {
                        context.Employees.Remove(employee);
                        await context.SaveChangesAsync();
                        LoadEmployees();
                        StatusLabel.Text = "Employee deleted successfully";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting employee: {ex.Message}", "Delete Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        else
        {
            MessageBox.Show("Please select an employee to delete.", "No Selection", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
    
    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadEmployees();
    }
    
    private void RefreshLogsButton_Click(object sender, RoutedEventArgs e)
    {
        LoadAuditLogs();
    }
    
    private void ExportLogsButton_Click(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Export audit logs to CSV",
            FileName = $"audit_logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };
        
        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                using var writer = new StreamWriter(saveFileDialog.FileName);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                
                var exportRecords = _auditEvents.Select(e => new
                {
                    Timestamp = e.Timestamp,
                    EventType = e.EventType,
                    Username = e.Employee?.Username ?? "",
                    DeviceHostname = e.Device?.Hostname ?? "",
                    Description = e.Description ?? ""
                }).ToList();
                
                csv.WriteRecords(exportRecords);
                
                MessageBox.Show($"Successfully exported {exportRecords.Count} audit log entries", "Export Complete", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting audit logs: {ex.Message}", "Export Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    public class EmployeeCsvRecord
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string? Role { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Password { get; set; }
    }
}