using System.Windows;

namespace DockerPsMonitor
{
    /// <summary>
    /// Interaction logic for LogOutputWindow.xaml
    /// </summary>
    public partial class LogOutputWindow : Window
    {
        public LogOutputWindow() : this("", "No data") { }

        public LogOutputWindow(string message, string title)
        {
            InitializeComponent();
            MainWindow.Title = title;
            MainTextBox.Text = message;
        }
    }
}
