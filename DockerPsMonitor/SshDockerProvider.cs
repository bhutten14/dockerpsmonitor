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

        public Task<(List<DockerProcessInfo>, string)> GetContainerInfoAsync(bool includeExitedContainers)
        {
            if (_client == null)
            {
                _client = CreateSshClient();
            }
            var rawOutput = "";
            string dockerCommandError = null;
            var updatedProcessInfos = new List<DockerProcessInfo>();

            var allFlag = includeExitedContainers ? "-a" : "";
            try
            {
                var command = _client.CreateCommand("docker ps --format \"{{json .}}\" " + allFlag);
                rawOutput = command.Execute();
                var jsonLines = rawOutput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
                updatedProcessInfos = jsonLines.Select(JsonConvert.DeserializeObject<DockerProcessInfo>).ToList();
            }
            catch
            {
                dockerCommandError = $"Error in executing docker command via SSH. [{rawOutput}]";
            }

            return Task.FromResult((updatedProcessInfos, dockerCommandError));
        }

        public string GetConnectionInfo()
        {
            var appReader = new AppSettingsReader();
            var sshAddress = (string)appReader.GetValue("SshAddress", typeof(string));
            return $"SSH: {sshAddress}";
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
