using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DockerPsMonitor
{
    public class DockerProvider : IDockerProvider
    {
        public async Task<List<DockerProcessInfo>> GetContainerInfoAsync(bool includeExitedContainers)
        {
            var allFlag = includeExitedContainers ? "-a" : "";
            var rawOutput = await DockerCliCommand.ExecuteDockerCommandAsync("ps --format \"{{json .}}\" " + allFlag);
            var jsonLines = await Task.Run(() => rawOutput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList());
            var updatedProcessInfos = await Task.Run(() => jsonLines.Select(JsonConvert.DeserializeObject<DockerProcessInfo>).ToList());
            return updatedProcessInfos;
        }

        public string GetConnectionInfo()
        {
            return "local CMD";
        }

        public Task<string> GetLogAsync(string containerId)
        {
            return DockerCliCommand.ExecuteDockerCommandAsync($"logs {containerId}");
        }

        public Task<string> StopAsync(string containerId)
        {
            return DockerCliCommand.ExecuteDockerCommandAsync($"stop {containerId}");
        }

        public Task<string> StartAsync(string containerId)
        {
            return DockerCliCommand.ExecuteDockerCommandAsync($"start {containerId}");
        }

        public Task<string> KillAsync(string containerId)
        {
            return DockerCliCommand.ExecuteDockerCommandAsync($"kill {containerId}");
        }

        public Task<string> RestartAsync(string containerId)
        {
            return DockerCliCommand.ExecuteDockerCommandAsync($"restart {containerId}");
        }

        public Task<string> RemoveAsync(string containerId)
        {
            return DockerCliCommand.ExecuteDockerCommandAsync($"rm {containerId}");
        }
    }
}
