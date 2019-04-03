using System.Collections.Generic;
using System.Threading.Tasks;

namespace DockerPsMonitor
{
    public interface IDockerProvider
    {
        Task<List<DockerProcessInfo>> GetContainerInfoAsync(bool includeExitedContainers);
        string GetConnectionInfo();
        string GetLog(string containerId);
        string Stop(string containerId);
        string Start(string containerId);
        string Kill(string containerId);
        string Restart(string containerId);
        string Remove(string containerId);
    }
}