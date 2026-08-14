using Azure.Messaging.ServiceBus;

using Up4All.Framework.MessageBus.ServiceBus.Extensions;

namespace Up4All.Framework.MessageBus.ServiceBus.Tests.Extensions
{
    [TestFixture]
    public class ServiceBusExtensionsTests
    {
        private const string FakeConnectionString =
            "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ZmFrZWtleQ==";

        [Test]
        public void CreateTopicProcessor_ReturnsProcessorForSubscription()
        {
            var client = new ServiceBusClient(FakeConnectionString);

            var processor = client.CreateTopicProcessor("topic-1", "sub-1", autoComplete: true);

            Assert.That(processor, Is.Not.Null);
            Assert.That(processor.EntityPath, Does.Contain("topic-1"));
            Assert.That(processor.EntityPath, Does.Contain("sub-1"));
            Assert.That(processor.AutoCompleteMessages, Is.True);
        }

        [Test]
        public void CreateQueueProcessor_ReturnsProcessorForQueue()
        {
            var client = new ServiceBusClient(FakeConnectionString);

            var processor = client.CreateQueueProcessor("queue-1", autoComplete: false);

            Assert.That(processor, Is.Not.Null);
            Assert.That(processor.EntityPath, Is.EqualTo("queue-1"));
            Assert.That(processor.AutoCompleteMessages, Is.False);
        }
    }
}
