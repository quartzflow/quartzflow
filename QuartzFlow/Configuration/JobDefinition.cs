using System;

namespace QuartzFlow
{
    public class JobDefinition : IEquatable<JobDefinition>
    {
        public string JobName { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        public RunSchedule RunSchedule { get; set; }
        public string ExecutableName { get; set; }
        public string Parameters { get; set; }
        public JobIdentifier RunOnCompletionOf { get; set; }
        public JobIdentifier RunOnSuccessOf { get; set; }
        public JobIdentifier RunOnFailureOf { get; set; }
        public int Retries { get; set; }
        public int WarnAfter { get; set; }
        public int TerminateAfter { get; set; }

        public bool Equals(JobDefinition other)
        {
            if (JobName == other.JobName &&
               Description == other.Description &&
               Group == other.Group &&
               RunSchedule.Equals(other.RunSchedule) &&
               ExecutableName == other.ExecutableName &&
               Parameters == other.Parameters &&
               RunOnCompletionOf.Equals(other.RunOnCompletionOf) &&
               RunOnSuccessOf.Equals(other.RunOnSuccessOf) &&
               RunOnFailureOf.Equals(other.RunOnFailureOf) &&
               Retries == other.Retries &&
               WarnAfter == other.WarnAfter &&
               TerminateAfter == other.TerminateAfter)
                return true;
            else
                return false;
        }
    }
}
