using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DockerPsMonitor
{
    public class DockerProvider : IDockerProvider
    {
        public Task<(List<DockerProcessInfo>, string)> GetContainerInfoAsync(bool includeExitedContainers)
        {
            var allFlag = includeExitedContainers ? "-a" : "";
            var rawOutput = "";
            var updatedProcessInfos = new List<DockerProcessInfo>();
            string dockerCommandError = null;
            try
            {
                rawOutput = DockerCliCommand.ExecuteDockerCommand("ps --format \"{{json .}}\" " + allFlag);
                var jsonLines = rawOutput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                updatedProcessInfos = jsonLines.Select(JsonConvert.DeserializeObject<DockerProcessInfo>).ToList();
            }
            catch
            {
                dockerCommandError = $"Error in executing docker command. [{rawOutput}]";
            }

            return Task.FromResult((updatedProcessInfos, dockerCommandError));
        }
    }
}
