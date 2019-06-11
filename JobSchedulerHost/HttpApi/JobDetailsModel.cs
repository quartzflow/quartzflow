using System.Collections.Generic;

namespace QuartzFlowHost.HttpApi
{
    public class JobDetailsModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string NextRunAt { get; set; }
        public string Status { get; set; }
        public SortedList<string, string> Properties { get; set; }
    }
}
