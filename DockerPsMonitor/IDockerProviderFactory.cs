namespace DockerPsMonitor
{
    public interface IDockerProviderFactory
    {
        IDockerProvider CreateDockerProvider(ConnectionItemViewModel connectionInfo);
    }

    class DockerProviderFactory : IDockerProviderFactory
    {
        public IDockerProvider CreateDockerProvider(ConnectionItemViewModel connectionInfo)
        {
            if (connectionInfo.Mode == ConnectionModeEnum.CMD)
            {
                return new DockerProvider();
            }
            return new SshDockerProvider(connectionInfo.Address, connectionInfo.UserName, connectionInfo.Password);
        }
    }
}
