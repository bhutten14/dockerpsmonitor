using System.Collections.Generic;

namespace DockerPsMonitor
{
    public interface IConnectionsRepository
    {
        List<ConnectionItemViewModel> GetAllConnections();
        bool SaveAllConnections(List<ConnectionItemViewModel> connections);
    }
}
