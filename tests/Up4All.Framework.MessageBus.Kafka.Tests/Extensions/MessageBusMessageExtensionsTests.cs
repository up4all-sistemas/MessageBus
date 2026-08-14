using System.Linq;
using System.Text.Json;

using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Kafka.Extensions;

namespace Up4All.Framework.MessageBus.Kafka.Tests.Extensions
{
    [TestFixture]
    public class MessageBusMessageExtensionsTests
    {
        [Test]
        public void ToKafkaMessage_UsesMessageIdAsKeyAndBodyAsValue()
        {
            var message = new MessageBusMessage();
            message.AddBody("payload");
            message.SetMessageId("key-1");

            var kafkaMessage = message.ToKafkaMessage<string>();

            Assert.That(kafkaMessage.Key, Is.EqualTo("key-1"));
            Assert.That(kafkaMessage.Value, Is.EqualTo(message.Body));
        }

        [Test]
        public void ToKafkaMessage_NoUserProperties_HeadersStayNull()
        {
            var message = new MessageBusMessage();
            message.AddBody("payload");

            var kafkaMessage = message.ToKafkaMessage<string>();

            Assert.That(kafkaMessage.Headers, Is.Null);
        }

        [Test]
        public void ToKafkaMessage_WithUserProperties_SerializesEachAsHeader()
        {
            var message = new MessageBusMessage();
            message.AddBody("payload");
            message.SetMessageId("key-1");
            message.AddUserProperty("k", "v");

            var kafkaMessage = message.ToKafkaMessage<string>();

            Assert.That(kafkaMessage.Headers, Is.Not.Null);
            var header = kafkaMessage.Headers!.Single(h => h.Key == "k");
            Assert.That(JsonSerializer.Deserialize<string>(header.GetValueBytes()), Is.EqualTo("v"));
        }

        [Test]
        public void ToKafkaMessageFromKeyStruct_UsesStructMessageIdAsKey()
        {
            var message = new MessageBusMessage();
            message.AddBody("payload");
            message.SetMessageIdFromStruct(42);

            var kafkaMessage = message.ToKafkaMessageFromKeyStruct<int>();

            Assert.That(kafkaMessage.Key, Is.EqualTo(42));
            Assert.That(kafkaMessage.Value, Is.EqualTo(message.Body));
        }

        [Test]
        public void ToKafkaMessageFromKeyStruct_WithUserProperties_SerializesEachAsHeader()
        {
            var message = new MessageBusMessage();
            message.AddBody("payload");
            message.SetMessageIdFromStruct(42);
            message.AddUserProperty("k", "v");

            var kafkaMessage = message.ToKafkaMessageFromKeyStruct<int>();

            Assert.That(kafkaMessage.Headers, Is.Not.Null);
        }
    }
}
