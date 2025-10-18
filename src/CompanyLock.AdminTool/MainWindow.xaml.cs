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
        
        // Initialize UI elements
        StartDatePicker.SelectedDate = DateTime.Now.AddDays(-7); // Default to last week
        EndDatePicker.SelectedDate = DateTime.Now;
        
        // Subscribe to events
        AuditLogsDataGrid.SelectionChanged += AuditLogsDataGrid_SelectionChanged;
        
        LoadEmployees();
        _ = LoadAuditLogs(); // Fire and forget for async method
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
    
    private async Task LoadAuditLogs()
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
            
            // Update statistics and load event types
            await UpdateLogStatistics();
            await LoadEventTypes();
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
    
    private async void RefreshLogsButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadAuditLogs();
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
    
    // Log Management Event Handlers
    private async void ClearAllLogsButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to delete ALL audit logs? This action cannot be undone!",
            "Confirm Clear All Logs",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                LogStatusLabel.Text = "Clearing all logs...";
                var deletedCount = await _authService.ClearAllLogsAsync();
                
                await LoadAuditLogs();
                await UpdateLogStatistics();
                
                LogStatusLabel.Text = $"Successfully deleted {deletedCount} logs";
                
                MessageBox.Show($"Successfully deleted {deletedCount} audit logs",
                    "Clear Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogStatusLabel.Text = "Error clearing logs";
                MessageBox.Show($"Error clearing logs: {ex.Message}",
                    "Clear Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    private async void CleanupOldLogsButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "This will delete all audit logs older than 30 days. Continue?",
            "Confirm Cleanup Old Logs",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                LogStatusLabel.Text = "Cleaning up old logs...";
                var deletedCount = await _authService.CleanupOldLogsAsync(30);
                
                await LoadAuditLogs();
                await UpdateLogStatistics();
                
                LogStatusLabel.Text = $"Cleanup completed - deleted {deletedCount} old logs";
                
                MessageBox.Show($"Successfully deleted {deletedCount} old audit logs (older than 30 days)",
                    "Cleanup Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogStatusLabel.Text = "Error during cleanup";
                MessageBox.Show($"Error during cleanup: {ex.Message}",
                    "Cleanup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    private async void DeleteByDateButton_Click(object sender, RoutedEventArgs e)
    {
        if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
        {
            MessageBox.Show("Please select both start and end dates",
                "Date Range Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        var startDate = StartDatePicker.SelectedDate.Value;
        var endDate = EndDatePicker.SelectedDate.Value.AddDays(1).AddMilliseconds(-1); // End of day
        
        var result = MessageBox.Show(
            $"Delete all logs from {startDate:dd/MM/yyyy} to {EndDatePicker.SelectedDate.Value:dd/MM/yyyy}?",
            "Confirm Delete by Date Range",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                LogStatusLabel.Text = "Deleting logs by date range...";
                var deletedCount = await _authService.DeleteLogsByDateRangeAsync(startDate, endDate);
                
                await LoadAuditLogs();
                await UpdateLogStatistics();
                
                LogStatusLabel.Text = $"Deleted {deletedCount} logs from date range";
                
                MessageBox.Show($"Successfully deleted {deletedCount} logs from selected date range",
                    "Delete Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogStatusLabel.Text = "Error deleting logs by date";
                MessageBox.Show($"Error deleting logs: {ex.Message}",
                    "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    private async void DeleteByTypeButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedEventType = EventTypeComboBox.SelectedItem?.ToString();
        
        if (string.IsNullOrEmpty(selectedEventType))
        {
            MessageBox.Show("Please select an event type",
                "Event Type Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        var result = MessageBox.Show(
            $"Delete all logs of type '{selectedEventType}'?",
            "Confirm Delete by Event Type",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                LogStatusLabel.Text = "Deleting logs by event type...";
                var deletedCount = await _authService.DeleteLogsByEventTypeAsync(selectedEventType);
                
                await LoadAuditLogs();
                await UpdateLogStatistics();
                await LoadEventTypes(); // Refresh event types
                
                LogStatusLabel.Text = $"Deleted {deletedCount} logs of type {selectedEventType}";
                
                MessageBox.Show($"Successfully deleted {deletedCount} logs of type '{selectedEventType}'",
                    "Delete Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogStatusLabel.Text = "Error deleting logs by type";
                MessageBox.Show($"Error deleting logs: {ex.Message}",
                    "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    private async void DeleteSelectedLogsButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedLogs = AuditLogsDataGrid.SelectedItems.Cast<AuditEvent>().ToList();
        
        if (!selectedLogs.Any())
        {
            MessageBox.Show("Please select one or more logs to delete",
                "No Logs Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        var result = MessageBox.Show(
            $"Delete {selectedLogs.Count} selected log(s)?",
            "Confirm Delete Selected Logs",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                LogStatusLabel.Text = "Deleting selected logs...";
                
                var config = ConfigurationManager.GetConfig();
                using var context = new LocalDbContext($"Data Source={config.DatabasePath}");
                
                var logIds = selectedLogs.Select(l => l.Id).ToList();
                var logsToDelete = context.AuditEvents.Where(e => logIds.Contains(e.Id));
                
                context.AuditEvents.RemoveRange(logsToDelete);
                await context.SaveChangesAsync();
                
                await LoadAuditLogs();
                await UpdateLogStatistics();
                
                LogStatusLabel.Text = $"Deleted {selectedLogs.Count} selected logs";
                
                MessageBox.Show($"Successfully deleted {selectedLogs.Count} selected logs",
                    "Delete Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogStatusLabel.Text = "Error deleting selected logs";
                MessageBox.Show($"Error deleting selected logs: {ex.Message}",
                    "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    private async Task LoadEventTypes()
    {
        try
        {
            var config = ConfigurationManager.GetConfig();
            using var context = new LocalDbContext($"Data Source={config.DatabasePath}");
            
            var eventTypes = await context.AuditEvents
                .Select(e => e.EventType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
            
            EventTypeComboBox.Items.Clear();
            foreach (var eventType in eventTypes)
            {
                EventTypeComboBox.Items.Add(eventType);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading event types: {ex.Message}",
                "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private async Task UpdateLogStatistics()
    {
        try
        {
            var logCount = await _authService.GetLogCountAsync();
            var dbSize = await _authService.GetDatabaseSizeAsync();
            
            LogCountLabel.Text = $"Total Logs: {logCount:N0}";
            DatabaseSizeLabel.Text = $"Database Size: {FormatBytes(dbSize)}";
            
            // Update selected count
            var selectedCount = AuditLogsDataGrid.SelectedItems.Count;
            SelectedCountLabel.Text = $"Selected: {selectedCount} logs";
        }
        catch
        {
            LogCountLabel.Text = "Total Logs: Error";
            DatabaseSizeLabel.Text = "Database Size: Error";
        }
    }
    
    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
        return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
    }
    
    private void AuditLogsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedCount = AuditLogsDataGrid.SelectedItems.Count;
        SelectedCountLabel.Text = $"Selected: {selectedCount} logs";
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