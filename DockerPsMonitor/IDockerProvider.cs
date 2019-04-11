using System.Collections.Generic;
using System.Threading.Tasks;

namespace DockerPsMonitor
{
    public interface IDockerProvider
    {
        Task<List<DockerProcessInfo>> GetContainerInfoAsync(bool includeExitedContainers);
        string GetConnectionInfo();
        Task<string> GetLogAsync(string containerId);
        Task<string> StopAsync(string containerId);
        Task<string> StartAsync(string containerId);
        Task<string> KillAsync(string containerId);
        Task<string> RestartAsync(string containerId);
        Task<string> RemoveAsync(string containerId);
    }
}