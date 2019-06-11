using System;

namespace QuartzFlow
{
    public class JobIdentifier : IEquatable<JobIdentifier>
    {
        public string Group { get; set; }
        public string JobName { get; set; }

        public string QuartzId => $"{Group}.{JobName}";

        public bool Equals(JobIdentifier other)
        {
            if (Group == other.Group && JobName == other.JobName)
                return true;
            else
                return false;
        }
    }
}
