using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSchedulerHost.HttpApi
{
    public class ActiveJobDetailsModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartedAt { get; set; }
        public double MinutesExecutingFor { get; set; }
        public int RetryCount { get; set; }
    }
}
