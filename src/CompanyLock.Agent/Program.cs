using CompanyLock.Core.Configuration;
using CompanyLock.LocalAuth.Services;
using CompanyLock.Agent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CompanyLock.Agent;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        // Configure as Windows Service
        builder.Services.AddWindowsService(options =>
        {
            options.ServiceName = "CompanyLockService";
        });
        
        // Load configuration
        var config = CompanyLock.Core.Configuration.ConfigurationManager.GetConfig();
        
        // Register services
        builder.Services.AddSingleton(config);
        builder.Services.AddSingleton(provider => new LocalAuthService($"Data Source={config.DatabasePath}"));
        builder.Services.AddHostedService<AgentWorker>();
        builder.Services.AddSingleton<SessionMonitorService>();
        builder.Services.AddSingleton<IdleMonitorService>();
        builder.Services.AddSingleton<HotkeyService>();
        builder.Services.AddSingleton<PipeServerService>();
        
        var host = builder.Build();
        
        await host.RunAsync();
    }
}