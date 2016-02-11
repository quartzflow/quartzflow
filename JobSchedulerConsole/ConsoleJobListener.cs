using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace JobSchedulerConsole
{
    public class ConsoleJobListener : IJobListener
    {
        public ConsoleJobListener()
        {
            Name = "ConsoleJobListener";
        }

        public void JobToBeExecuted(IJobExecutionContext context)
        {
            //
        }

        public void JobExecutionVetoed(IJobExecutionContext context)
        {
            //
        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            Console.WriteLine("In Listener:" + context.Get("Output"));
        }

        public string Name { get ; private set; }
    }
}
