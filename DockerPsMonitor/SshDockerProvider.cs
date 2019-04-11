using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Renci.SshNet;

namespace DockerPsMonitor
{
    public class SshDockerProvider : IDockerProvider
    {
        private SshClient _client;

        public async Task<List<DockerProcessInfo>> GetContainerInfoAsync(bool includeExitedContainers)
        {
            if (_client == null)
            {
                _client = CreateSshClient();
            }

            var allFlag = includeExitedContainers ? "-a" : "";
            var command = _client.CreateCommand("docker ps --format \"{{json .}}\" " + allFlag);
            var rawOutput = await Task.Run(() => command.Execute());
            var jsonLines = await Task.Run(() => rawOutput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList());
            var updatedProcessInfos = await Task.Run(() => jsonLines.Select(JsonConvert.DeserializeObject<DockerProcessInfo>).ToList());
            return updatedProcessInfos;
        }

        public string GetConnectionInfo()
        {
            var appReader = new AppSettingsReader();
            var sshAddress = (string)appReader.GetValue("SshAddress", typeof(string));
            return $"SSH: {sshAddress}";
        }

        public Task<string> GetLogAsync(string containerId)
        {
            var command = _client.CreateCommand($"docker logs {containerId}");
            return Task.Run(() => command.Execute());
        }

        public Task<string>  StopAsync(string containerId)
        {
            var command = _client.CreateCommand($"docker stop {containerId}");
            return Task.Run(() => command.Execute());
        }

        public Task<string>  StartAsync(string containerId)
        {
            var command = _client.CreateCommand($"docker start {containerId}");
            return Task.Run(() => command.Execute());
        }

        public Task<string>  KillAsync(string containerId)
        {
            var command = _client.CreateCommand($"docker kill {containerId}");
            return Task.Run(() => command.Execute());
        }

        public Task<string>  RestartAsync(string containerId)
        {
            var command = _client.CreateCommand($"docker restart {containerId}");
            return Task.Run(() => command.Execute());
        }

        public Task<string>  RemoveAsync(string containerId)
        {
            var command = _client.CreateCommand($"docker rm {containerId}");
            return Task.Run(() => command.Execute());
        }

        private SshClient CreateSshClient()
        {
            var appReader = new AppSettingsReader();
            var sshAddress = (string)appReader.GetValue("SshAddress", typeof(string));
            var sshUsername = (string)appReader.GetValue("SshUsername", typeof(string));
            var sshPassword = (string)appReader.GetValue("SshPassword", typeof(string));

            _client = new SshClient(sshAddress, sshUsername, sshPassword);
            _client.Connect();
            return _client;
        }
    }
}
