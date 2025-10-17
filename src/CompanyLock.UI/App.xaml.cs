using System.Windows;

namespace CompanyLock.UI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Create and show the lock screen
        var lockScreen = new LockScreen();
        lockScreen.Show();
    }
}