using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace JobSchedulerConsole
{
    public class HelloJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Greetings from HelloJob!");
            //var commandLine = (string)context.JobDetail.JobDataMap["CommandLine"];

            //var consoleRunner = new Process();
            //consoleRunner.StartInfo.CreateNoWindow = true;
            //consoleRunner.StartInfo.FileName = commandLine;
            //consoleRunner.StartInfo.UseShellExecute = false;
            //consoleRunner.StartInfo.RedirectStandardOutput = true;
            //consoleRunner.StartInfo.RedirectStandardError = true;
            //consoleRunner.Start();

            //var output = consoleRunner.StandardOutput.ReadToEnd();

            //consoleRunner.WaitForExit();

            //context.Put("Output", output);
        }
    }
}
