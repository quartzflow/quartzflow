using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using QuartzFlow.Jobs;
using QuartzFlow.QuartzExtensions;
using NUnit.Framework;
using Quartz;
using Quartz.Impl;

namespace QuartzFlow.Tests.Jobs
{
    [TestFixture]
    public class ConsoleJobFixture
    {
        private ConsoleJob _successJob;
        private ConsoleJob _failJob;
        private ConsoleJob _errorExitCodeJob;
        private string _outputFile;

        [SetUp]
        public void Setup()
        {
            _outputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output.txt");
            SetupSuccessJob();
            SetupFailureJob();
            SetupErrorExitCodeJob();
            File.Delete(_outputFile);
        }

        [Test]
        public void HasCorrectAttributes()
        {
            var attributes = _successJob.GetType().GetCustomAttributes(false);
            Assert.AreEqual("Quartz.PersistJobDataAfterExecutionAttribute", attributes[0].ToString());
            Assert.AreEqual("Quartz.DisallowConcurrentExecutionAttribute", attributes[1].ToString());
        }

        [Test]
        public void Execute_WithValidParameters_WillRunJobAndWaitForCompletion()
        {
            var context = GetJobContext();
            _successJob.Execute(context);

            int processId = (int) context.Get(Constants.FieldNames.ProcessId);
            Assert.That(processId > 0);
            ArgumentException ex = Assert.Throws<ArgumentException>(() => System.Diagnostics.Process.GetProcessById(processId));
            Assert.AreEqual($"Process with an Id of {processId} is not running.", ex.Message);
            Assert.AreEqual(JobExecutionStatus.Succeeded, context.Result);
        }

        [Test]
        public void Execute_WithValidParameters_WillRunJobAndCaptureStandardOutputResults()
        {
            var context = GetJobContext();
            _successJob.Execute(context);

            string output = context.ReadOutputBufferContents();
            Assert.That(output.Contains($"About to run {context.JobDetail.Key} at {DateTime.Now.ToString("dd/MM/yyyy")}"));
            Assert.That(output.Contains($"FileName: {_successJob.ExecutableName}, Parameters: {_successJob.Parameters}"));
            Assert.That(output.Contains("Sleeping for 1000 ms"));
            Assert.That(output.Contains("Returning success code."));

            Assert.AreEqual(output, File.ReadAllText(_outputFile));
            Assert.AreEqual(JobExecutionStatus.Succeeded, context.Result);
        }

        [Test]
        public void Execute_OnFailureToStartJob_WillLogError()
        {
            var context = GetJobContext();

            var exception = Assert.Throws<JobExecutionException>(() => _failJob.Execute(context));
            string output = context.ReadOutputBufferContents();
            Assert.That(output.Contains($"About to run {context.JobDetail.Key} at {DateTime.Now.ToString("dd/MM/yyyy")}"));
            Assert.That(output.Contains($"FileName: {_failJob.ExecutableName}, Parameters: {_failJob.Parameters}"));
            Assert.That(output.Contains("Error executing job - The system cannot find the file specified"));
        }

        [Test]
        public void Execute_OnFailureAndWithinRetryAttempts_WillSetupForRetry()
        {
            var context = GetJobContext();

            var exception = Assert.Throws<JobExecutionException>(() => _failJob.Execute(context));
            Assert.AreEqual(true, exception.RefireImmediately);
            Assert.AreEqual(JobExecutionStatus.Retrying, context.Result);
        }

        [Test]
        public void Execute_AfterMaxRetriesExceeded_WillFail()
        {
            var context = GetJobContext(3);

            _failJob.Execute(context);

            Assert.AreEqual(JobExecutionStatus.Failed, context.Result);
            string output = context.ReadOutputBufferContents();
            Assert.That(output.Contains($"No more retries available for job {context.JobDetail.Key}.  Setting status to Failed."));
        }

        [Test]
        public void Execute_OnErrorExitCodeAndWithinRetryAttempts_WillSetupForRetry()
        {
            var context = GetJobContext();

            var exception = Assert.Throws<JobExecutionException>(() => _errorExitCodeJob.Execute(context));
            Assert.AreEqual(true, exception.RefireImmediately);
            Assert.AreEqual(JobExecutionStatus.Retrying, context.Result);
        }

        [Test]
        public void Execute_OnErrorExitCodeAndAfterMaxRetriesExceeded_WillThrowExceptionAndFail()
        {
            var context = GetJobContext(3);

            _errorExitCodeJob.Execute(context);
            Assert.AreEqual(JobExecutionStatus.Failed, context.Result);
            string output = context.ReadOutputBufferContents();
            Assert.That(output.Contains($"No more retries available for job {context.JobDetail.Key}.  Setting status to Failed."));
        }

        private void SetupSuccessJob()
        {
            _successJob = new ConsoleJob
            {
                ExecutableName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestApp\TestApp.exe"),
                Parameters = "success 1000",
                OutputFile = _outputFile
            };
        }

        private void SetupFailureJob()
        {
            _failJob = new ConsoleJob
            {
                ExecutableName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestApp\Missing.exe"),
                OutputFile = _outputFile,
                MaxRetries = 2
            };
        }

        private void SetupErrorExitCodeJob()
        {
            _errorExitCodeJob = new ConsoleJob
            {
                ExecutableName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestApp\TestApp.exe"),
                Parameters = "failure 1000",
                OutputFile = _outputFile,
                MaxRetries = 2
            };
        }

        private TestJobExecutionContext GetJobContext(int refireCount = 0)
        {
            var context = new TestJobExecutionContext
            {
                RefireCount = refireCount,
                JobDetail = new JobDetailImpl("TestJob", typeof(ConsoleJob)),
                MergedJobDataMap = new JobDataMap()
            };

            return context;
        }

        
    }

    internal class TestJobExecutionContext : IJobExecutionContext
    {
        private Dictionary<string, object> _datamap;

        public TestJobExecutionContext()
        {
            _datamap = new Dictionary<string, object>();
        }

        public void Put(object key, object objectValue)
        {
            _datamap[key.ToString()] = objectValue;
        }

        public object Get(object key)
        {
            return _datamap[key.ToString()];
        }

        public IScheduler Scheduler { get; private set; }
        public ITrigger Trigger { get; private set; }
        public ICalendar Calendar { get; private set; }
        public bool Recovering { get; private set; }
        public TriggerKey RecoveringTriggerKey { get; private set; }
        public int RefireCount { get; set; }
        public JobDataMap MergedJobDataMap { get;  set; }
        public IJobDetail JobDetail { get;  set; }
        public IJob JobInstance { get; private set; }
        public DateTimeOffset FireTimeUtc { get; private set; }
        public DateTimeOffset? ScheduledFireTimeUtc { get; private set; }
        public DateTimeOffset? PreviousFireTimeUtc { get; private set; }
        public DateTimeOffset? NextFireTimeUtc { get; private set; }
        public string FireInstanceId { get; private set; }
        public object Result { get; set; }
        public TimeSpan JobRunTime { get; private set; }
        public CancellationToken CancellationToken { get; }
    }
}
