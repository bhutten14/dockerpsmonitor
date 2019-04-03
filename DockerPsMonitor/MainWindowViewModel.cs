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

        public MainWindowViewModel(IDockerProvider dockerProvider)
        {
            _dockerProvider = dockerProvider;
            RefreshRate = 2;
            ShowExitedContainers = true;
            ViewLogCommand = new DelegateCommand(async () => await OnViewLog(), CanViewLog)
                                    .ObservesProperty(() => SelectedContainer);
            CopyIdCommand = new DelegateCommand(OnCopyId, CanViewLog).ObservesProperty(() => SelectedContainer);
            StopCommand = new DelegateCommand(async () => await OnStop(), CanViewLog).ObservesProperty(() => SelectedContainer);
            StartCommand = new DelegateCommand(async () => await OnStart(), CanViewLog).ObservesProperty(() => SelectedContainer);
            KillCommand = new DelegateCommand(async () => await OnKill(), CanViewLog).ObservesProperty(() => SelectedContainer);
            RestartCommand = new DelegateCommand(async () => await OnRestart(), CanViewLog).ObservesProperty(() => SelectedContainer);
            RemoveCommand = new DelegateCommand(async () => await OnRemove(), CanViewLog).ObservesProperty(() => SelectedContainer);
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

        public ICommand ViewLogCommand { get; set; }

        public ICommand CopyIdCommand { get; set; }

        public ICommand StopCommand { get; set; }

        public ICommand StartCommand { get; set; }

        public ICommand KillCommand { get; set; }

        public ICommand RestartCommand { get; set; }

        public ICommand RemoveCommand { get; set; }

        public string DockerCommandError
        {
            get => _dockerCommandError;
            set => SetProperty(ref _dockerCommandError, value);
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

        private Task OnViewLog()
        {
            var containerId = SelectedContainer.ID;
            var rawOutput = "";
            try
            {
                rawOutput = _dockerProvider.GetLog(containerId);
            }
            catch
            {
                DockerCommandError = $"Error in getting log. [{rawOutput}]";
                return Task.CompletedTask;
            }
            var newLogWindow = new LogOutputWindow(rawOutput, SelectedContainer.Names);
            newLogWindow.Show();
            return Task.CompletedTask;
        }

        private Task OnRemove()
        {
            var containerId = SelectedContainer.ID;
            var rawOutput = "";
            try
            {
                rawOutput = _dockerProvider.Remove(containerId);
            }
            catch
            {
                DockerCommandError = $"Error removing container. [{rawOutput}]";
            }
            return Task.CompletedTask;
        }

        private Task OnRestart()
        {
            var containerId = SelectedContainer.ID;
            var rawOutput = "";
            try
            {
                rawOutput = _dockerProvider.Restart(containerId);
            }
            catch
            {
                DockerCommandError = $"Error restarting container. [{rawOutput}]";
            }
            return Task.CompletedTask;
        }

        private Task OnKill()
        {
            var containerId = SelectedContainer.ID;
            var rawOutput = "";
            try
            {
                rawOutput = _dockerProvider.Kill(containerId);
            }
            catch
            {
                DockerCommandError = $"Error killing container. [{rawOutput}]";
            }
            return Task.CompletedTask;
        }

        private Task OnStart()
        {
            var containerId = SelectedContainer.ID;
            var rawOutput = "";
            try
            {
                rawOutput = _dockerProvider.Start(containerId);
            }
            catch
            {
                DockerCommandError = $"Error starting container. [{rawOutput}]";
            }
            return Task.CompletedTask;
        }

        private Task OnStop()
        {
            var containerId = SelectedContainer.ID;
            var rawOutput = "";
            try
            {
                rawOutput = _dockerProvider.Stop(containerId);
            }
            catch
            {
                DockerCommandError = $"Error stopping container. [{rawOutput}]";
            }
            return Task.CompletedTask;
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
    }
}
