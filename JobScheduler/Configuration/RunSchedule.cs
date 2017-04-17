using System;

namespace JobScheduler
{
    public class RunSchedule : IEquatable<RunSchedule>
    {
        public string RunAt { get; set; }
        public RunOn RunOn { get; set; }
        public string ExclusionCalendar { get; set; }
        public string Timezone { get; set; }

        public bool Equals(RunSchedule other)
        {
            if (RunAt == other.RunAt &&
                RunOn.Equals(other.RunOn) &&
                ExclusionCalendar == other.ExclusionCalendar &&
                Timezone == other.Timezone)
                return true;
            else
                return false;
        }
    }
}
