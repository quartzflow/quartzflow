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

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                JobKey key = context.JobDetail.Key;
                JobDataMap dataMap = context.MergedJobDataMap;

                if (MaxRetries > 0 && context.RefireCount >= MaxRetries)
                {
                    var e = new JobExecutionException("Retries exceeded") {RefireImmediately = false};
                    //unschedule it so that it doesn't run again
                    context.Result = JobExecutionStatus.Failed;
                    throw e;
                }

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
                    }
                };

                _output = "-------------------------";
                _output += $"About to run {key} at {DateTime.Now:dd/MM/yyyy HH:mm:ss.fff}{Environment.NewLine}";
                _output += $"FileName: {ExecutableName}, Parameters: {Parameters}{Environment.NewLine}";

                consoleRunner.Start();

                context.Put(Constants.FieldNames.ProcessId, consoleRunner.Id);
                _output += consoleRunner.StandardOutput.ReadToEnd();

                consoleRunner.WaitForExit();

                context.Result = JobExecutionStatus.Succeeded;
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
                    _output += $"Error executing job - {ex.Message}{Environment.NewLine}{ex.StackTrace}";
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
            }
        }
    }
}
