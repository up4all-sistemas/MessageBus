using Up4All.Framework.MessageBus.Kafka.Options;

namespace Up4All.Framework.MessageBus.Kafka.Tests.Options
{
    [TestFixture]
    public class KafkaMessageBusOptionsTests
    {
        [Test]
        public void SchemaRegistryUrl_DefaultsToNull()
        {
            var options = new KafkaMessageBusOptions();

            Assert.That(options.SchemaRegistryUrl, Is.Null);
        }

        [Test]
        public void SchemaRegistryUrl_IsSettable()
        {
            var options = new KafkaMessageBusOptions { SchemaRegistryUrl = "http://schema-registry:8081" };

            Assert.That(options.SchemaRegistryUrl, Is.EqualTo("http://schema-registry:8081"));
        }

        [Test]
        public void InheritsBaseMessageBusOptionsDefaults()
        {
            var options = new KafkaMessageBusOptions();

            Assert.That(options.ConnectionAttempts, Is.EqualTo(1));
            Assert.That(options.ConnectionString, Is.Null);
        }
    }
}
