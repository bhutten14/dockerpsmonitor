using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Mvvm;

namespace DockerPsMonitor
{
    public class MainWindowViewModel : BindableBase
    {
        private ObservableCollection<DockerProcessInfo> _processInfos = new ObservableCollection<DockerProcessInfo>();
        private readonly DispatcherTimer _timer;
        private int _refreshRate;
        private bool _showExitedContainers;
        private DockerProcessInfo _selectedContainer;
        private string _dockerCommandError;
        private IDockerProvider _dockerProvider;
        private ObservableCollection<ConnectionItemViewModel> _connectionItems;
        private ConnectionItemViewModel _selectedConnectionItem;
        private readonly IDockerProviderFactory _dockerProviderFactory;
        private bool _dockerProviderAvailable;
        private IConnectionsRepository _connectionsRepository;
        private bool _showConnectionsPanel;
        private bool _showRefreshing;
        private bool _showContainerListEmpty;

        public MainWindowViewModel(IDockerProviderFactory dockerProviderFactory, IConnectionsRepository connectionsRepository)
        {
            _dockerProviderFactory = dockerProviderFactory;
            _connectionsRepository = connectionsRepository;
            ReadConnections();
            RefreshRate = 2;
            ShowExitedContainers = true;
            ViewLogCommand = new DelegateCommand(OnViewLog, CanViewLog).ObservesProperty(() => SelectedContainer);
            CopyIdCommand = new DelegateCommand(OnCopyId, CanViewLog).ObservesProperty(() => SelectedContainer);
            StopCommand = new DelegateCommand(OnStop, CanViewLog).ObservesProperty(() => SelectedContainer);
            StartCommand = new DelegateCommand(OnStart, CanViewLog).ObservesProperty(() => SelectedContainer);
            KillCommand = new DelegateCommand(OnKill, CanViewLog).ObservesProperty(() => SelectedContainer);
            RestartCommand = new DelegateCommand(OnRestart, CanViewLog).ObservesProperty(() => SelectedContainer);
            RemoveCommand = new DelegateCommand(OnRemove, CanViewLog).ObservesProperty(() => SelectedContainer);
            AddConnectionItemCommand = new DelegateCommand(OnAddConnectionItem);
            RemoveConnectionItemCommand = new DelegateCommand(OnRemoveConnectionItem, CanRemoveConnectionItem).ObservesProperty(() => SelectedConnectionItem);
            ConnectionInfoCommand = new DelegateCommand(OnConnectionInfo);
            ReconnectCommand = new DelegateCommand(OnReconnectCommand);
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(0) };
            _timer.Tick += OnTimerElapsed;
            _timer.Start();
        }

        public ObservableCollection<DockerProcessInfo> ProcessInfos
        {
            get => _processInfos;
            set => SetProperty(ref _processInfos, value);
        }

        public int RefreshRate
        {
            get => _refreshRate;
            set
            {
                SetProperty(ref _refreshRate, value);
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Interval = TimeSpan.FromSeconds(RefreshRate);
                    _timer.Start();
                }
            }
        }

        public bool ShowExitedContainers
        {
            get => _showExitedContainers;
            set => SetProperty(ref _showExitedContainers, value);
        }

        public DockerProcessInfo SelectedContainer
        {
            get => _selectedContainer;
            set => SetProperty(ref _selectedContainer, value);
        }

        public ObservableCollection<ConnectionItemViewModel> ConnectionItems
        {
            get => _connectionItems;
            set => SetProperty(ref _connectionItems, value);
        }

        public ICommand ViewLogCommand { get; set; }

        public ICommand CopyIdCommand { get; set; }

        public ICommand StopCommand { get; set; }

        public ICommand StartCommand { get; set; }

        public ICommand KillCommand { get; set; }

        public ICommand RestartCommand { get; set; }

        public ICommand RemoveCommand { get; set; }

        public ICommand AddConnectionItemCommand { get; set; }

        public ICommand RemoveConnectionItemCommand { get; set; }

        public ICommand ConnectionInfoCommand { get; set; }

        public ICommand ReconnectCommand { get; set; }

        public string DockerCommandError
        {
            get => _dockerCommandError;
            set => SetProperty(ref _dockerCommandError, value);
        }

        public ConnectionItemViewModel SelectedConnectionItem
        {
            get => _selectedConnectionItem;
            set
            {
                ShowRefreshing = true;
                ShowContainerListEmpty = false;
                _connectionsRepository.SaveAllConnections(ConnectionItems.ToList());
                _timer?.Stop();
                _processInfos.Clear();
                SetProperty(ref _selectedConnectionItem, value);
                try
                {
                    _dockerProvider = _dockerProviderFactory.CreateDockerProvider(value);
                    DockerProviderAvailable = true;
                    _timer?.Start();
                }
                catch
                {
                    ShowRefreshing = false;
                    ShowContainerListEmpty = false;
                    DockerProviderAvailable = false;
                }
                RaisePropertyChanged(nameof(ConnectionInfo));
            }
        }

        public bool DockerProviderAvailable
        {
            get => _dockerProviderAvailable;
            set => SetProperty(ref _dockerProviderAvailable, value);
        }

        public bool ShowConnectionsPanel
        {
            get => _showConnectionsPanel;
            set => SetProperty(ref _showConnectionsPanel, value);
        }

        public bool ShowRefreshing
        {
            get => _showRefreshing;
            set => SetProperty(ref _showRefreshing, value);
        }

        public bool ShowContainerListEmpty
        {
            get => _showContainerListEmpty;
            set => SetProperty(ref _showContainerListEmpty, value);
        }

        public string ConnectionInfo => _dockerProvider.GetConnectionInfo();

        private void OnCopyId()
        {
            Clipboard.SetText(SelectedContainer.ID);
        }

        private bool CanViewLog()
        {
            return SelectedContainer != null;
        }

        private async void OnViewLog()
        {
            var newLogWindow = new LogOutputWindow("Loading log lines ...", SelectedContainer.Names);
            newLogWindow.Show();
            var containerId = SelectedContainer.ID;
            var rawOutput = "";
            try
            {
                rawOutput = await _dockerProvider.GetLogAsync(containerId);
                newLogWindow.MainTextBox.Text = rawOutput;
            }
            catch
            {
                DockerCommandError = $"Error getting log. [{rawOutput}]";
                newLogWindow.Close();
            }
        }

        private async void OnRemove()
        {
            var containerId = SelectedContainer.ID;
            var rawOutput = "";
            try
            {
                rawOutput = await _dockerProvider.RemoveAsync(containerId);
            }
            catch
            {
                DockerCommandError = $"Error removing container. [{rawOutput}]";
            }
        }

        private async void OnRestart()
        {
            var containerId = SelectedContainer.ID;
            var rawOutput = "";
            try
            {
                rawOutput = await _dockerProvider.RestartAsync(containerId);
            }
            catch
            {
                DockerCommandError = $"Error restarting container. [{rawOutput}]";
            }
        }

        private async void OnKill()
        {
            var containerId = SelectedContainer.ID;
            var rawOutput = "";
            try
            {
                rawOutput = await _dockerProvider.KillAsync(containerId);
            }
            catch
            {
                DockerCommandError = $"Error killing container. [{rawOutput}]";
            }
        }

        private async void OnStart()
        {
            var containerId = SelectedContainer.ID;
            var rawOutput = "";
            try
            {
                rawOutput = await _dockerProvider.StartAsync(containerId);
            }
            catch
            {
                DockerCommandError = $"Error starting container. [{rawOutput}]";
            }
        }

        private async void OnStop()
        {
            var containerId = SelectedContainer.ID;
            var rawOutput = "";
            try
            {
                rawOutput = await _dockerProvider.StopAsync(containerId);
            }
            catch
            {
                DockerCommandError = $"Error stopping container. [{rawOutput}]";
            }
        }

        private async void OnTimerElapsed(object sender, EventArgs eventArgs)
        {
            _timer.Stop();
            await RefreshProcessInfoAsync();
            _timer.Interval = TimeSpan.FromSeconds(RefreshRate);
            _timer.Start();
        }

        private async Task RefreshProcessInfoAsync()
        {
            if (!ShowContainerListEmpty && ProcessInfos.Count == 0)
            {
                ShowRefreshing = true;
            }
            var updatedProcessInfos = new List<DockerProcessInfo>();
            DockerCommandError = null;
            try
            {
                updatedProcessInfos = await _dockerProvider.GetContainerInfoAsync(ShowExitedContainers);
            }
            catch (Exception exception)
            {
                DockerCommandError = $"Error in executing docker command. [{exception.Message}]";
            }
            var addedItems = updatedProcessInfos.Except(ProcessInfos, new DockerProcessInfoComparer()).ToList();
            var removedItems = ProcessInfos.Except(updatedProcessInfos, new DockerProcessInfoComparer()).ToList();
            var updatedItems = updatedProcessInfos.Where(p1 => ProcessInfos.Any(p2 => p1.ID.Equals(p2.ID))).ToList();
            foreach (var dockerProcessInfo in removedItems)
            {
                var toBeRemoved = ProcessInfos.Single(p => p.ID.Equals(dockerProcessInfo.ID));
                ProcessInfos.Remove(toBeRemoved);
            }
            foreach (var dockerProcessInfo in addedItems)
            {
                ProcessInfos.Add(dockerProcessInfo);
            }
            foreach (var dockerProcessInfo in updatedItems)
            {
                var toBeUpdatedItem = ProcessInfos.Single(p => p.ID.Equals(dockerProcessInfo.ID));
                toBeUpdatedItem.Status = dockerProcessInfo.Status;
                toBeUpdatedItem.Names = dockerProcessInfo.Names;
                toBeUpdatedItem.Ports = dockerProcessInfo.Ports;
            }
            ShowRefreshing = false;
            ShowContainerListEmpty = ProcessInfos.Count == 0;
        }

        private void OnAddConnectionItem()
        {
            var newItem = new ConnectionItemViewModel(_connectionsRepository, ConnectionModeEnum.SSH, "New item", "");
            ConnectionItems.Add(newItem);
            SelectedConnectionItem = newItem;
        }

        private void OnRemoveConnectionItem()
        {
            if (SelectedConnectionItem != null && SelectedConnectionItem.Mode != ConnectionModeEnum.CMD)
            {
                ConnectionItems.Remove(SelectedConnectionItem);
                SelectedConnectionItem = ConnectionItems.Last();
            }
        }

        private bool CanRemoveConnectionItem()
        {
            return SelectedConnectionItem?.Mode != ConnectionModeEnum.CMD;
        }
        
        private void OnConnectionInfo()
        {
            ShowConnectionsPanel = !ShowConnectionsPanel;
        }

        private void OnReconnectCommand()
        {
            SelectedConnectionItem = SelectedConnectionItem;
        }

        private void ReadConnections()
        {
            //Default one connection is available, knowing the CMD mode connection
            _connectionItems = new ObservableCollection<ConnectionItemViewModel>
            {
                new ConnectionItemViewModel(_connectionsRepository, ConnectionModeEnum.CMD, "local cmd", "local cmd")
            };
            _connectionItems.AddRange(_connectionsRepository.GetAllConnections());
            SelectedConnectionItem = _connectionItems.First();
        }
    }
}
