using System.Windows;

namespace Various.Sample;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var bootstrapper = new Bootstrapper();
        var mainWindow = new MainWindow(bootstrapper);

        mainWindow.Show();
    }
}
