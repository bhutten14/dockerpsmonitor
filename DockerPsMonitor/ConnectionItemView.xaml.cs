using System;
using System.Windows;
using System.Windows.Controls;

namespace DockerPsMonitor
{
    /// <summary>
    /// Interaction logic for ConnectionItemControl.xaml
    /// </summary>
    public partial class ConnectionItemView : UserControl
    {
        public ConnectionItemView()
        {
            InitializeComponent();
            PwdBox.Password = "ABCDEFGHIJKLMNOP";
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }
            var pwd = (PasswordBox) sender;
            var connectionItem = (ConnectionItemViewModel)DataContext;
            connectionItem.Password = pwd.SecurePassword;
        }
    }
}
