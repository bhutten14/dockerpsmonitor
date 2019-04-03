using System.Collections.Generic;
using System.Threading.Tasks;

namespace DockerPsMonitor
{
    public interface IDockerProvider
    {
        Task<List<DockerProcessInfo>> GetContainerInfoAsync(bool includeExitedContainers);
        string GetConnectionInfo();
        string GetLog(string containerId);
    }
}