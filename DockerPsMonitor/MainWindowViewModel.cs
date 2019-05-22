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
        private readonly IDockerProvider _dockerProvider;
        private ObservableCollection<ConnectionItemData> _connectionItems;
        private ConnectionItemData _selectedConnectionItem;

        public MainWindowViewModel(IDockerProvider dockerProvider)
        {
            _connectionItems = new ObservableCollection<ConnectionItemData> { new ConnectionItemData
            {
                Mode = ConnectionModeEnum.CMD,
                Name = "local cmd",
                Address = "local cmd"
            }};
            SelectedConnectionItem = _connectionItems.First();
            _dockerProvider = dockerProvider;
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

        public ObservableCollection<ConnectionItemData> ConnectionItems
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

        public string DockerCommandError
        {
            get => _dockerCommandError;
            set => SetProperty(ref _dockerCommandError, value);
        }

        public ConnectionItemData SelectedConnectionItem
        {
            get => _selectedConnectionItem;
            set => SetProperty(ref _selectedConnectionItem, value);
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
        }

        private void OnAddConnectionItem()
        {
            var newItem = new ConnectionItemData
            {
                Name = "New item", 
                Mode = ConnectionModeEnum.SSH
            };
            ConnectionItems.Add(newItem);
            SelectedConnectionItem = newItem;
        }
    }
}
