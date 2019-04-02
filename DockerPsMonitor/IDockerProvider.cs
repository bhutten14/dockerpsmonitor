using System.Collections.Generic;
using System.Threading.Tasks;

namespace DockerPsMonitor
{
    public interface IDockerProvider
    {
        Task<(List<DockerProcessInfo>, string)> GetContainerInfoAsync(bool includeExitedContainers);
        string GetConnectionInfo();
    }
}