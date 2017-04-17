using Quartz;
using Quartz.Listener;

namespace JobScheduler.Listeners
{
    public class ConsoleJobListener : JobListenerSupport
    {
        public override void JobToBeExecuted(IJobExecutionContext context)
        {
            Log.Info($"-----About to run '{context.JobDetail.Key}'");
        }

        public override void JobExecutionVetoed(IJobExecutionContext context)
        {
            Log.Info($"-----Run of '{context.JobDetail.Key}' was vetoed!");
        }

        public override void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            Log.Info("In Listener: " + context.Get("Output"));
            Log.Info("---------------------");
        }

        public override string Name => "ConsoleJobListener";
    }
}
