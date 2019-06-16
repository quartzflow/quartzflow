using System;
using System.Diagnostics;
using System.IO;
using QuartzFlow.QuartzExtensions;
using Quartz;
using System.Threading.Tasks;

namespace QuartzFlow.Jobs
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

        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                JobKey key = context.JobDetail.Key;
                _jobKey = key.ToString();
                JobDataMap dataMap = context.MergedJobDataMap;
                context.ClearOutputBuffer();

                if ((context.RefireCount > 0) && (context.RefireCount > MaxRetries))
                {
                    WriteImmediatelyToOutputLog(context, $"No more retries available for job {key}.  Setting status to Failed.");
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

                    WriteImmediatelyToOutputLog(context, "--------------------------------------------------------------------------");
                    WriteImmediatelyToOutputLog(context, $"Attempt {context.RefireCount+1} of {MaxRetries+1}: About to run {key} at {DateTime.Now:dd/MM/yyyy HH:mm:ss.fff}");
                    WriteImmediatelyToOutputLog(context, $"FileName: {ExecutableName}, Parameters: {Parameters}");

                    consoleRunner.Exited += ConsoleRunner_Exited;

                    consoleRunner.Start();

                    context.Put(Constants.FieldNames.ProcessId, consoleRunner.Id);
                    WriteImmediatelyToOutputLog(context, consoleRunner.StandardOutput.ReadToEnd());

                    consoleRunner.WaitForExit();

                    if (consoleRunner.ExitCode == 0)
                        context.Result = JobExecutionStatus.Succeeded;
                    else
                        throw new Exception("Process returned an error code");

                }

                return Task.CompletedTask;

            }
            //Only JobExecutionExceptions are expected from jobs
            catch (Exception ex)
            {
                bool retry = true;

                if (ex is JobExecutionException exception)
                {
                    retry = exception.RefireImmediately;
                }

                if (retry)
                {
                    WriteImmediatelyToOutputLog(context, $"Error executing job - {ex.Message}{Environment.NewLine}{ex.StackTrace}");
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
                if (GetOutputBufferContents().Length > 0)
                {
                    WriteImmediatelyToOutputLog(context, GetOutputBufferContents());
                    ClearOutputBuffer();
                }
            }
        }

        private void ConsoleRunner_Exited(object sender, EventArgs e)
        {
            Process job = (Process)sender;

            if (job.ExitCode != 0)
            {
                WriteToOutputBuffer($"Exit event: Job {_jobKey} exited at {job.ExitTime.ToLongTimeString()} with an exit code of {job.ExitCode}");
            }
        }
        private void WriteImmediatelyToOutputLog(IJobExecutionContext context, string s)
        {
            File.AppendAllText(OutputFile, s + Environment.NewLine);
            context.AppendToOutputBuffer(s);
        }

        private void WriteToOutputBuffer(string s)
        {
            _output += s;
        }

        private string GetOutputBufferContents()
        {
            return _output ?? string.Empty;
        }

        private void ClearOutputBuffer()
        {
            _output = string.Empty;
        }

    }
}
