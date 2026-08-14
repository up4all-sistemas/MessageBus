using Confluent.Kafka;

using System.Text;
using System.Text.Json;

using Up4All.Framework.MessageBus.Kafka.Extensions;
using Up4All.Framework.MessageBus.Kafka.Interfaces;

namespace Up4All.Framework.MessageBus.Kafka.Tests.Extensions
{
    [TestFixture]
    public class KafkaExtensionsTests
    {
        private const string FakeBootstrapServers = "localhost:1";

        private class FakeTopicClient : IKafkaTopicClient { }

        private class FakeSubscriptionClient : IKafkaSubscriptionClient { }

        [Test]
        public void ActivitySource_IsASingletonAcrossCalls()
        {
            // Regression test: this used to be a property that built a brand-new
            // ActivitySource (with its own assembly-name/version lookup) on every access.
            var first = KafkaExtensions.ActivitySource;
            var second = KafkaExtensions.ActivitySource;

            Assert.That(first, Is.SameAs(second));
        }

        [Test]
        public void Meter_IsASingletonAcrossCalls()
        {
            var first = KafkaExtensions.Meter;
            var second = KafkaExtensions.Meter;

            Assert.That(first, Is.SameAs(second));
        }

        [Test]
        public void CreateProducer_BuildsProducerForReferenceKey()
        {
            var producer = new FakeTopicClient().CreateProducer<string>(FakeBootstrapServers);

            Assert.That(producer, Is.Not.Null);
            producer.Dispose();
        }

        [Test]
        public void CreateProducerForStructKey_BuildsProducerForValueKey()
        {
            var producer = new FakeTopicClient().CreateProducerForStructKey<int>(FakeBootstrapServers);

            Assert.That(producer, Is.Not.Null);
            producer.Dispose();
        }

        [Test]
        public void CreateConfig_SetsBootstrapServersGroupIdAndEarliestOffset()
        {
            var config = new FakeSubscriptionClient().CreateConfig(FakeBootstrapServers, "sub-1");

            Assert.That(config.BootstrapServers, Is.EqualTo(FakeBootstrapServers));
            Assert.That(config.GroupId, Is.EqualTo("sub-1"));
            Assert.That(config.AutoOffsetReset, Is.EqualTo(AutoOffsetReset.Earliest));
        }

        [Test]
        public void CreateConsumerForStructKey_BuildsConsumerWithGivenGroupId()
        {
            var consumer = new FakeSubscriptionClient().CreateConsumerForStructKey<int>(FakeBootstrapServers, "group-1");

            Assert.That(consumer, Is.Not.Null);
            consumer.Dispose();
        }

        [Test]
        public void ToReceivedMessage_MapsKeyBodyAndHeaders()
        {
            var message = new Message<string, byte[]>
            {
                Key = "key-1",
                Value = Encoding.UTF8.GetBytes("payload"),
                Headers = []
            };
            message.Headers.Add("custom-header", JsonSerializer.SerializeToUtf8Bytes("header-value"));

            var received = message.ToReceivedMessage();

            Assert.That(received.GetBody(), Is.EqualTo("payload"));
            Assert.That(received.GetMessageIdForClass<string>(), Is.EqualTo("key-1"));
            Assert.That(received.UserProperties["custom-header"]?.ToString(), Is.EqualTo("header-value"));
        }

        [Test]
        public void ToReceivedMessage_NoHeaders_DoesNotThrow()
        {
            var message = new Message<string, byte[]> { Key = "key-1", Value = Encoding.UTF8.GetBytes("payload") };

            Assert.DoesNotThrow(() => message.ToReceivedMessage());
        }

        [Test]
        public void ToReceivedMessageWithStructKey_MapsStructKeyAndBody()
        {
            var message = new Message<int, byte[]> { Key = 42, Value = Encoding.UTF8.GetBytes("payload") };

            var received = message.ToReceivedMessageWithStructKey();

            Assert.That(received.GetBody(), Is.EqualTo("payload"));
            Assert.That(received.GetMessageIdForStruct<int>(), Is.EqualTo(42));
        }
    }
}
