using System.Windows;

namespace DockerPsMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow() { }

        public MainWindow(IDockerProvider dockerProvider)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(dockerProvider);
        }
    }
}
