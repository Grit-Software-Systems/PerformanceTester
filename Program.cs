using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using Tools;

namespace PerfRunner
{
    class Program
    {
        static int threadCount = int.Parse(Config("threads", "1"));
        static int iterations = int.Parse(Config("iterations", "1"));

        static string suiteName = "";
        static string appInsightsInstrumentationkey = "";
        static string testRunnerPath = "";


        static void Main(string[] args)
        {
            ParseArgs(args);

            TestCases testCases = new TestCases();

            string[] suites = suiteName.Split(',');

            for (int j = 0; j < suites.Length; j++)
            {
                Guid overallId = Guid.NewGuid();
                TelemetryLog log = new TelemetryLog(appInsightsInstrumentationkey);
                log.TrackEvent("starting with the test suite: " + suites[j], "correlationId", overallId.ToString());
                for (int iter = 0; iter < iterations; iter++)
                {
                    List<WaitHandle> waitHandles = new List<WaitHandle>();
                    for (int tc = 0; tc < threadCount; tc++)
                    {
                        ManualResetEvent waitHandle = new ManualResetEvent(false);
                        waitHandles.Add(waitHandle);
                        TestCases ts = new TestCases();
                        Thread thread = new Thread(() => { ts.Execute(testRunnerPath, suites[j], appInsightsInstrumentationkey, overallId.ToString()); waitHandle.Set(); });
                        thread.Start();
                    }
                    WaitHandle.WaitAll(waitHandles.ToArray());
                }
                log.TrackEvent("Completing the test suite: " + suites[j], "correlationId", overallId.ToString());
            }

        }


        static void ParseArgs(string[] args)
        {
            ProcessArg(args, "suiteNamePrefix", ref suiteName);
            ProcessArg(args, "executablePathPrefix", ref testRunnerPath);
            ProcessArg(args, "appInsightsInstrumentationKeyPrefix", ref appInsightsInstrumentationkey);

            if (!testRunnerPath.Contains(".exe"))
            {
                testRunnerPath = Directory.GetFiles(testRunnerPath, "b2ctestcaserunner.exe").FirstOrDefault();
            }

            string value = "";
            ProcessArg(args, "threadsPrefix", ref value);
            if (!string.IsNullOrEmpty(value))
                threadCount = int.Parse(value);

            ProcessArg(args, "iterationsPrefix", ref value);
            if (!string.IsNullOrEmpty(value))
                iterations = int.Parse(value);
        }


        static void ProcessArg(string[] args, string prefixName, ref string field)
        {
            string prefix = Config(prefixName);
            string value = args.Where(a => a.ToLower().Contains(prefix.ToLower())).FirstOrDefault();
            if (!string.IsNullOrEmpty(value))
            {
                field = value.Substring(prefix.Length);
            }
        }

        static string Config(string key, string defaultValue = "")
        {
            string value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }
    }
}
