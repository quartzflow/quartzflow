using Topshelf;

namespace JobSchedulerHost
{
    public class Program
    {
        private static void Main(string[] args)
        {
            HostFactory.Run(x =>                                 
            {
                x.Service<SchedulerHost>(s =>                        
                {
                    s.ConstructUsing(name => new SchedulerHost());     
                    s.WhenStarted(tc => tc.Start());              
                    s.WhenStopped(tc => tc.Stop());               
                });
                x.RunAsLocalSystem();                            

                x.SetDescription("JobScheduler Host");        
                x.SetDisplayName("JobScheduler Host");                       
                x.SetServiceName("JobSchedulerHost");                       
            });
        }





       
    }


}
