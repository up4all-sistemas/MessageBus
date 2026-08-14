using Up4All.Framework.MessageBus.ServiceBus.Consts;

namespace Up4All.Framework.MessageBus.ServiceBus.Tests.Consts
{
    [TestFixture]
    public class ServiceBusConstsTests
    {
        [Test]
        public void Constants_HaveExpectedValues()
        {
            Assert.That(ServiceBusConsts.Provider, Is.EqualTo("servicebus"));
            Assert.That(ServiceBusConsts.OpenTelemetrySourceName, Is.EqualTo("Azure ServiceBus Up4All MessageBus"));
        }
    }
}
