using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Renci.SshNet;

namespace DockerPsMonitor
{
    public class SshDockerProvider : IDockerProvider
    {
        private SshClient _client;
        private string _address;

        public SshDockerProvider(string address, string userName, SecureString password)
        {
            _client = CreateSshClient(address, userName, password);
            _address = address;
        }

        public async Task<List<DockerProcessInfo>> GetContainerInfoAsync(bool includeExitedContainers)
        {
            if (_client == null)
            {
                return new List<DockerProcessInfo>();
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
            return $"SSH: {_address}";
        }

        public Task<string> GetLogAsync(string containerId)
        {
            var command = _client.CreateCommand($"docker logs {containerId}");
            return Task.Run(() => command.Execute());
        }

        public Task<string> StopAsync(string containerId)
        {
            var command = _client.CreateCommand($"docker stop {containerId}");
            return Task.Run(() => command.Execute());
        }

        public Task<string> StartAsync(string containerId)
        {
            var command = _client.CreateCommand($"docker start {containerId}");
            return Task.Run(() => command.Execute());
        }

        public Task<string> KillAsync(string containerId)
        {
            var command = _client.CreateCommand($"docker kill {containerId}");
            return Task.Run(() => command.Execute());
        }

        public Task<string> RestartAsync(string containerId)
        {
            var command = _client.CreateCommand($"docker restart {containerId}");
            return Task.Run(() => command.Execute());
        }

        public Task<string> RemoveAsync(string containerId)
        {
            var command = _client.CreateCommand($"docker rm {containerId}");
            return Task.Run(() => command.Execute());
        }

        private SshClient CreateSshClient(string address, string userName, SecureString password)
        {
            if (password == null)
            {
                throw new ArgumentException("No password available cannot make SSH connection.");
            }

            var pwdClear = new NetworkCredential(string.Empty, password).Password;
            _client = new SshClient(address, userName, pwdClear);
            _client.Connect();
            return _client;
        }
    }
}
