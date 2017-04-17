using System;

namespace JobScheduler
{
    public class RunOn : IEquatable<RunOn>
    {
        public string Days { get; set; }
        public string Calendar { get; set; }

        public bool Equals(RunOn other)
        {
            if (Days == other.Days && Calendar == other.Calendar)
                return true;
            else
                return false;
        }
    }
}
