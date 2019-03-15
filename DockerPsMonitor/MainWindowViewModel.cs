using System;
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

        public string DockerCommandError
        {
            get => _dockerCommandError;
            set => SetProperty(ref _dockerCommandError, value);
        }

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
            string rawOutput = "";
            try
            {
                rawOutput = DockerCliCommand.ExecuteDockerCommand($"logs {containerId}");
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

        private async void OnTimerElapsed(object sender, EventArgs eventArgs)
        {
            _timer.Stop();
            await RefreshProcessInfoAsync();
            _timer.Interval = TimeSpan.FromSeconds(RefreshRate);
            _timer.Start();
        }

        private async Task RefreshProcessInfoAsync()
        {
            var (updatedProcessInfos, errorMessage) = await _dockerProvider.GetContainerInfoAsync(ShowExitedContainers);
            DockerCommandError = errorMessage;
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
