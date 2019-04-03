using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DockerPsMonitor
{
    public class DockerProvider : IDockerProvider
    {
        public Task<List<DockerProcessInfo>> GetContainerInfoAsync(bool includeExitedContainers)
        {
            var allFlag = includeExitedContainers ? "-a" : "";
            var rawOutput = DockerCliCommand.ExecuteDockerCommand("ps --format \"{{json .}}\" " + allFlag);
            var jsonLines = rawOutput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var updatedProcessInfos = jsonLines.Select(JsonConvert.DeserializeObject<DockerProcessInfo>).ToList();
            return Task.FromResult(updatedProcessInfos);
        }

        public string GetConnectionInfo()
        {
            return "local cmd";
        }

        public string GetLog(string containerId)
        {
            return DockerCliCommand.ExecuteDockerCommand($"logs {containerId}");
        }
    }
}
