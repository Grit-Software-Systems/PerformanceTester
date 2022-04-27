using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace PerfRunner
{
    class TestCases
    {
        public void Execute(
            string exePath,
            string suiteName,
            string appInsightsInstrumentationKey = "",
            string parentCorrelationId = "")
        {
            Dictionary<string, string> env = new Dictionary<string, string>();
            env.Add("appInsightsInstrumentationKey", appInsightsInstrumentationKey);

            string jsonSuite = suiteName;
            string pCorrelationId = "parentCorrelationId:" + parentCorrelationId;

            StartProcess(exePath, $"{suiteName} {pCorrelationId}", env);
        }


        void StartProcess(string executable, string arguments = "", Dictionary<string, string> env = null, string workingDirectory = "")
        {
            ProcessStartInfo proc = new System.Diagnostics.ProcessStartInfo();
            proc.FileName = @"C:\windows\system32\cmd.exe";
            proc.UseShellExecute = false;
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                proc.WorkingDirectory = workingDirectory;
            }
            else
            {
                string dir = Path.GetDirectoryName(executable);
                proc.WorkingDirectory = Path.GetDirectoryName(executable);
            }
            proc.Arguments = $"/c {executable} {arguments}";

            foreach (string key in env.Keys)
            {
                proc.Environment.Add(key, env[key]);
            }

            Process p = Process.Start(proc);
            p.WaitForExit();
        }

    }
}
