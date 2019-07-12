namespace DockerPsMonitor
{
    public interface IDockerProviderFactory
    {
        IDockerProvider CreateDockerProvider(ConnectionItemViewModel connectionInfo);
    }
}
