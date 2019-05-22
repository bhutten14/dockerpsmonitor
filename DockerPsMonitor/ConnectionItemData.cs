using System.Security;
using Prism.Mvvm;

namespace DockerPsMonitor
{
    public class ConnectionItemData : BindableBase
    {
        private string _name;
        private string _address;
        private string _userName;
        private ConnectionModeEnum _mode;

        public ConnectionModeEnum Mode
        {
            get => _mode;
            set => SetProperty(ref _mode, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public SecureString Password { get; set; }
    }
}
