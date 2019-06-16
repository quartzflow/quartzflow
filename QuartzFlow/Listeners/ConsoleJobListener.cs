using System;
using System.Threading;
using System.Threading.Tasks;
using NLog.Fluent;
using QuartzFlow.QuartzExtensions;
using Quartz;
using Quartz.Listener;
using Quartz.Logging.LogProviders;

namespace QuartzFlow.Listeners
{
    public class ConsoleJobListener : JobListenerSupport
    {
        public override Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            Log.Info($"-----About to run '{context.JobDetail.Key}'");
            return Task.CompletedTask;
        }

        public override Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            Log.Info($"-----Run of '{context.JobDetail.Key}' was vetoed!");
            return Task.CompletedTask;
        }

        public override Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = new CancellationToken())
        {
            Log.Info("-----Execution log:" + Environment.NewLine + context.ReadOutputBufferContents());
            Log.Info("---------------------");
            return Task.CompletedTask;
        }

        public override string Name => "ConsoleJobListener";
    }
}
