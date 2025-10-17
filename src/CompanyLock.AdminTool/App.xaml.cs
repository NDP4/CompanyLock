using System.Windows;
using System.Threading.Tasks;
using CompanyLock.LocalAuth.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.IO;

namespace CompanyLock.AdminTool;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Check for command line arguments
        if (e.Args.Length > 0 && e.Args[0] == "--fix-timestamps")
        {
            Task.Run(async () => await FixTimestampsAsync());
            Shutdown();
            return;
        }
        
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }
    
    private async Task FixTimestampsAsync()
    {
        try
        {
            Console.WriteLine("Starting timestamp conversion...");
            
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = Path.Combine(appDataPath, "CompanyLock", "local.db");
            var connectionString = $"Data Source={dbPath}";
            
            using var context = new LocalDbContext(connectionString);
            
            // Get all audit events that might have UTC timestamps
            var events = await context.AuditEvents.ToListAsync();
            var updatedCount = 0;
            
            Console.WriteLine($"Found {events.Count} audit events to check...");
            
            foreach (var auditEvent in events)
            {
                // Convert UTC timestamp to local time
                // Assuming stored time is UTC, convert to local
                var localTime = auditEvent.Timestamp.ToLocalTime();
                
                // Only update if there's a significant difference (more than 1 hour)
                if (Math.Abs((auditEvent.Timestamp - localTime).TotalHours) > 1)
                {
                    auditEvent.Timestamp = localTime;
                    updatedCount++;
                }
            }
            
            if (updatedCount > 0)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"Updated {updatedCount} timestamps from UTC to local time.");
            }
            else
            {
                Console.WriteLine("No timestamps needed conversion.");
            }
            
            Console.WriteLine("Timestamp conversion completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fixing timestamps: {ex.Message}");
        }
    }
}