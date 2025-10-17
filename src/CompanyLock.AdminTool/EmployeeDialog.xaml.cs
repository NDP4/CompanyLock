using System.Windows;
using System.Windows.Controls;
using CompanyLock.Core.Configuration;
using CompanyLock.Core.Models;
using CompanyLock.LocalAuth.Data;
using CompanyLock.LocalAuth.Services;
using Microsoft.EntityFrameworkCore;

namespace CompanyLock.AdminTool;

public partial class EmployeeDialog : Window
{
    private readonly Employee? _employee;
    private readonly bool _isEdit;
    
    public EmployeeDialog(Employee? employee = null)
    {
        InitializeComponent();
        
        _employee = employee;
        _isEdit = employee != null;
        
        if (_isEdit && _employee != null)
        {
            Title = "Edit Employee";
            LoadEmployeeData();
        }
        else
        {
            Title = "Add Employee";
            RoleComboBox.SelectedIndex = 0; // Default to User
            IsActiveCheckBox.IsChecked = true;
        }
    }
    
    private void LoadEmployeeData()
    {
        if (_employee == null) return;
        
        UsernameTextBox.Text = _employee.Username;
        FullNameTextBox.Text = _employee.FullName ?? "";
        DepartmentTextBox.Text = _employee.Department ?? "";
        RoleComboBox.SelectedItem = RoleComboBox.Items.Cast<ComboBoxItem>()
            .FirstOrDefault(item => item.Content.ToString() == _employee.Role);
        IsActiveCheckBox.IsChecked = _employee.IsActive;
        
        if (_isEdit)
        {
            UsernameTextBox.IsReadOnly = true; // Don't allow username changes
        }
    }
    
    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!ValidateInput())
                return;
                
            var config = ConfigurationManager.GetConfig();
            using var context = new LocalDbContext($"Data Source={config.DatabasePath}");
            
            if (_isEdit && _employee != null)
            {
                // Update existing employee
                var employee = await context.Employees.FindAsync(_employee.Id);
                if (employee == null)
                {
                    MessageBox.Show("Employee not found in database.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                employee.FullName = FullNameTextBox.Text.Trim();
                employee.Department = DepartmentTextBox.Text.Trim();
                employee.Role = ((ComboBoxItem)RoleComboBox.SelectedItem).Content.ToString() ?? "User";
                employee.IsActive = IsActiveCheckBox.IsChecked ?? false;
                employee.LastSyncAt = DateTime.Now;
                
                // Update password if provided
                if (!string.IsNullOrEmpty(PasswordBox.Password))
                {
                    var salt = PasswordHelper.GenerateSalt();
                    var hash = PasswordHelper.HashPassword(PasswordBox.Password, salt);
                    employee.Salt = salt;
                    employee.PasswordHash = hash;
                }
            }
            else
            {
                // Create new employee
                var username = UsernameTextBox.Text.Trim();
                
                // Check if username already exists
                var existingEmployee = await context.Employees
                    .FirstOrDefaultAsync(e => e.Username == username);
                if (existingEmployee != null)
                {
                    MessageBox.Show("Username already exists. Please choose a different username.", "Duplicate Username", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                var password = PasswordBox.Password;
                if (string.IsNullOrEmpty(password))
                {
                    password = "password123"; // Default password
                }
                
                var salt = PasswordHelper.GenerateSalt();
                var hash = PasswordHelper.HashPassword(password, salt);
                
                var employee = new Employee
                {
                    Username = username,
                    PasswordHash = hash,
                    Salt = salt,
                    FullName = FullNameTextBox.Text.Trim(),
                    Department = DepartmentTextBox.Text.Trim(),
                    Role = ((ComboBoxItem)RoleComboBox.SelectedItem).Content.ToString() ?? "User",
                    IsActive = IsActiveCheckBox.IsChecked ?? false,
                    CreatedAt = DateTime.Now,
                    LastSyncAt = DateTime.Now
                };
                
                context.Employees.Add(employee);
            }
            
            await context.SaveChangesAsync();
            
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving employee: {ex.Message}", "Save Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
        {
            MessageBox.Show("Username is required.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            UsernameTextBox.Focus();
            return false;
        }
        
        if (RoleComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a role.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            RoleComboBox.Focus();
            return false;
        }
        
        // Validate password if provided
        if (!string.IsNullOrEmpty(PasswordBox.Password) || !string.IsNullOrEmpty(ConfirmPasswordBox.Password))
        {
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return false;
            }
            
            if (PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return false;
            }
        }
        else if (!_isEdit)
        {
            // For new employees, password is required
            MessageBox.Show("Password is required for new employees.", "Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            PasswordBox.Focus();
            return false;
        }
        
        return true;
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}