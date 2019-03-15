using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerPsMonitor
{
    public static class DockerCliCommand
    {
        public static string ExecuteDockerCommand(string commandArgs)
        {
            var processInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "Docker.exe",
                Arguments = commandArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            var sb = new StringBuilder();
            var process = new Process { StartInfo = processInfo };
            process.OutputDataReceived += (e, o) => OnDataReceived(o.Data);
            process.ErrorDataReceived += (e, o) => OnDataReceived(o.Data);

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            void OnDataReceived(string data)
            {
                if (!String.IsNullOrEmpty(data))
                {
                    data = data.EndsWith(System.Environment.NewLine) || data.EndsWith("\n") ? data : data + Environment.NewLine;
                    sb.Append(data);
                }
            }

            process.WaitForExit();
            process.Close();
            var output = sb.ToString();
            return output;
        }
    }
}
