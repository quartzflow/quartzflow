using Newtonsoft.Json;
using NUnit.Framework;

namespace JobScheduler.Tests
{
    [TestFixture()]
    public class RunScheduleFixture
    {
        [Test]
        public void CanSerializeAndDeserializeAsJson()
        {
            var firstRunSchedule = new RunSchedule() { ExclusionCalendar = "Something", RunAt = "01:23", RunOn = new RunOn() {Calendar = "A", Days = "B"}, Timezone = "NZST"};

            var serializedRunSchedule = JsonConvert.SerializeObject(firstRunSchedule);

            var deserializedRunSchedule = JsonConvert.DeserializeObject<RunSchedule>(serializedRunSchedule);

            Assert.AreEqual(firstRunSchedule, deserializedRunSchedule, "RunSchedule objects are not equivalent");
        }
    }
}