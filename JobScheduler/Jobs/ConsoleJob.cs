using System;
using System.Diagnostics;
using System.IO;
using Quartz;

namespace JobScheduler.Jobs
{
    [PersistJobDataAfterExecution]  
    [DisallowConcurrentExecution]
    public class ConsoleJob : IJob
    {
        //These values are injected by the JobFactory
        public string ExecutableName { get; set; }
        public string Parameters { get; set; }
        public string OutputFile { get; set; }
        public int MaxRetries { get; set; }

        private string _output;
        private string _jobKey;

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                JobKey key = context.JobDetail.Key;
                _jobKey = key.ToString();
                JobDataMap dataMap = context.MergedJobDataMap;

                if ((context.RefireCount > 0) && (context.RefireCount > MaxRetries))
                {
                    _output += $"No more retries available for job {key}.  Setting status to Failed.{Environment.NewLine}";
                    context.Result = JobExecutionStatus.Failed;
                }
                else
                {
                    var consoleRunner = new Process
                    {
                        StartInfo =
                        {
                            CreateNoWindow = false,
                            FileName = ExecutableName,
                            Arguments = Parameters,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        },
                        EnableRaisingEvents = true,
                    };

                    _output += "--------------------------------------------------------------------------";
                    _output += $"Attempt {context.RefireCount+1} of {MaxRetries+1}: About to run {key} at {DateTime.Now:dd/MM/yyyy HH:mm:ss.fff}{Environment.NewLine}";
                    _output += $"FileName: {ExecutableName}, Parameters: {Parameters}{Environment.NewLine}";

                    consoleRunner.Exited += ConsoleRunner_Exited;

                    consoleRunner.Start();

                    context.Put(Constants.FieldNames.ProcessId, consoleRunner.Id);
                    _output += consoleRunner.StandardOutput.ReadToEnd();

                    consoleRunner.WaitForExit();

                    if (consoleRunner.ExitCode == 0)
                        context.Result = JobExecutionStatus.Succeeded;
                    else
                        throw new Exception("Process returned an error code");

                }
            }
            //Only JobExecutionExceptions are expected from jobs
            catch (Exception ex)
            {
                bool retry = true;

                var exception = ex as JobExecutionException;
                if (exception != null)
                {
                    retry = exception.RefireImmediately;
                }

                if (retry)
                {
                    _output += $"Error executing job - {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";
                    context.Result = JobExecutionStatus.Retrying;
                }
                else
                {
                    context.Result = JobExecutionStatus.Failed;
                }

                throw new JobExecutionException(ex) {RefireImmediately = retry};
            }
            finally
            {
                File.AppendAllText(OutputFile, _output);
                context.Put(Constants.FieldNames.StandardOutput, _output);  
                _output = String.Empty;
            }
        }

        private void ConsoleRunner_Exited(object sender, EventArgs e)
        {
            Process job = (Process)sender;

            if (job.ExitCode != 0)
            {
                _output += $"Exit event: Job {_jobKey} exited at {job.ExitTime.ToLongTimeString()} with an exit code of {job.ExitCode} {Environment.NewLine}";
            }
        }
    }
}
