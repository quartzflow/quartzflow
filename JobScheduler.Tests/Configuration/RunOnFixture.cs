using Newtonsoft.Json;
using NUnit.Framework;

namespace QuartzFlow.Tests
{
    [TestFixture()]
    public class RunOnFixture
    {
        [Test]
        public void CanSerializeAndDeserializeAsJson()
        {
            var firstRunOn = new RunOn() { Calendar = "AAA", Days = "BBB"};

            var serializedRunOn = JsonConvert.SerializeObject(firstRunOn);

            var deserializedRunOn = JsonConvert.DeserializeObject<RunOn>(serializedRunOn);

            Assert.AreEqual(firstRunOn, deserializedRunOn, "RunOn objects are not equivalent");
        }
    }
}
