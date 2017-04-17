using NUnit.Framework;
using Newtonsoft.Json;

namespace JobScheduler.Tests
{
    [TestFixture]
    public class JobDefinitionFixture
    {
        [Test]
        public void CanSerializeAndDeserializeAsJson()
        {
            JobDefinition jobA = new JobDefinition()
            {
                JobName = "SomeJob",
                Description = "It does stuff",
                Group = "SOD",
                RunSchedule = new RunSchedule() { ExclusionCalendar = "None", RunAt = "11", RunOn = new RunOn() { Calendar = "Everyday", Days = "M" }, Timezone = "AEST" },
                ExecutableName = "Hello",
                Parameters = "None",
                Retries = 3,
                RunOnCompletionOf = new JobIdentifier() { Group = "SOD", JobName = "a"},
                RunOnFailureOf = new JobIdentifier() { Group = "SOD", JobName = "b" },
                RunOnSuccessOf = new JobIdentifier() { Group = "SOD", JobName = "c" },
                TerminateAfter = 2,
                WarnAfter = 4
            };

            var serializedJob = JsonConvert.SerializeObject(jobA);

            JobDefinition deserializedJob = JsonConvert.DeserializeObject<JobDefinition>(serializedJob);

            Assert.AreEqual(jobA, deserializedJob, "JobDefinitions are not equivalent");
        }
    }
}
