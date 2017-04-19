using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobSchedulerHost.HttpApi
{
    public class JobDetailsModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string NextRunAt { get; set; }
    }
}
