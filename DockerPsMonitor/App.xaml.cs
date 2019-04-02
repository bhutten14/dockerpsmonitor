using System;
using System.Configuration;
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
            var appReader = new AppSettingsReader();
            var mode = (string)appReader.GetValue("Mode", typeof(string));
            IDockerProvider dockerProvider = null;
            if (mode.Equals("CMD", StringComparison.OrdinalIgnoreCase))
            {
                dockerProvider = new DockerProvider();
            }
            else if (mode.Equals("SSH", StringComparison.OrdinalIgnoreCase))
            {
                dockerProvider = new SshDockerProvider();
            }

            var mainWindow = new MainWindow(dockerProvider);
            mainWindow.ShowDialog();
        }
    }
}
