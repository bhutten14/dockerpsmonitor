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

        public Task<List<DockerProcessInfo>> GetContainerInfoAsync(bool includeExitedContainers)
        {
            if (_client == null)
            {
                _client = CreateSshClient();
            }

            var allFlag = includeExitedContainers ? "-a" : "";
            var command = _client.CreateCommand("docker ps --format \"{{json .}}\" " + allFlag);
            var rawOutput = command.Execute();
            var jsonLines = rawOutput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var updatedProcessInfos = jsonLines.Select(JsonConvert.DeserializeObject<DockerProcessInfo>).ToList();
            return Task.FromResult(updatedProcessInfos);
        }

        public string GetConnectionInfo()
        {
            var appReader = new AppSettingsReader();
            var sshAddress = (string)appReader.GetValue("SshAddress", typeof(string));
            return $"SSH: {sshAddress}";
        }

        public string GetLog(string containerId)
        {
            var command = _client.CreateCommand($"docker logs {containerId}");
            return command.Execute();
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
