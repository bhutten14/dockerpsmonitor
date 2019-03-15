using System;
using Prism.Mvvm;

namespace DockerPsMonitor
{
    public class DockerProcessInfo : BindableBase
    {
        private string _status;
        private string _id;
        private string _names;
        private string _ports;
        private ContainerStatus _containerStatus;

        public string ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Names
        {
            get => _names;
            set => SetProperty(ref _names, value);
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                if (_status.IndexOf("unhealthy", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ContainerStatus = ContainerStatus.Unhealthy;
                }
                else if (_status.IndexOf("healthy", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ContainerStatus = ContainerStatus.Healthy;
                }
                else if (_status.IndexOf("starting", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ContainerStatus = ContainerStatus.Starting;
                }
                else if (_status.IndexOf("exited", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ContainerStatus = ContainerStatus.Exited;
                }
                else
                {
                    ContainerStatus = ContainerStatus.Unknown;
                }
                OnPropertyChanged(nameof(Status));
            }
        }

        public string Ports
        {
            get => _ports;
            set => SetProperty(ref _ports, value);
        }

        public ContainerStatus ContainerStatus
        {
            get => _containerStatus;
            set => SetProperty(ref _containerStatus, value);
        }
    }
}
