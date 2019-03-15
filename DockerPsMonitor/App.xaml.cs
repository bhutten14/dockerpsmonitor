using System.Windows;

namespace DockerPsMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var dockerProvider = new DockerProvider();
            var mainWindow = new MainWindow(dockerProvider);
            mainWindow.ShowDialog();
        }
    }
}
