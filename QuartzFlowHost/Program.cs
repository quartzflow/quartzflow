using System;
using System.IO;
using Topshelf;

namespace QuartzFlowHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            HostFactory.Run(x =>                                 
            {
                x.Service<SchedulerHost>(s =>                        
                {
                    s.ConstructUsing(() => new SchedulerHost());     
                    s.WhenStarted(tc => tc.Start());              
                    s.WhenStopped(tc => tc.Stop());               
                });                         

                x.SetDescription("QuartzFlow Host");        
                x.SetDisplayName("QuartzFlow Host");                       
                x.SetServiceName("QuartzFlowHost");                       
            });
        }





       
    }


}
