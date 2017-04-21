using System.Collections.Generic;

namespace JobSchedulerHost.HttpApi
{
    public class JobDetailsModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string NextRunAt { get; set; }
        public SortedList<string, string> Properties { get; set; }
    }
}
