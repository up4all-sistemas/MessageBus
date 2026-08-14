using System.Text;

using RabbitMQ.Client;

using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests.Extensions
{
    [TestFixture]
    public class ReceivedMessageExtensionsTests
    {
        [Test]
        public void PopulateUserProperties_NoHeaders_DoesNothing()
        {
            var message = new ReceivedMessage();
            var props = new BasicProperties();

            message.PopulateUserProperties(props);

            Assert.That(message.UserProperties, Is.Empty);
        }

        [Test]
        public void PopulateUserProperties_WithHeaders_CopiesConvertedValues()
        {
            var message = new ReceivedMessage();
            var props = new BasicProperties
            {
                Headers = new Dictionary<string, object?>
                {
                    { "string-header", "plain-value" },
                    { "bytes-header", Encoding.UTF8.GetBytes("decoded-value") }
                }
            };

            message.PopulateUserProperties(props);

            Assert.That(message.UserProperties["string-header"], Is.EqualTo("plain-value"));
            Assert.That(message.UserProperties["bytes-header"], Is.EqualTo("decoded-value"));
        }

        [Test]
        public void PopulateUserProperties_WithValidCorrelationId_SetsCorrelationId()
        {
            var message = new ReceivedMessage();
            var correlationId = Guid.NewGuid();
            var props = new BasicProperties
            {
                CorrelationId = correlationId.ToString(),
                Headers = new Dictionary<string, object?> { { "h", "v" } }
            };

            message.PopulateUserProperties(props);

            Assert.That(message.GetCorrelationId(), Is.EqualTo(correlationId));
        }

        [Test]
        public void PopulateUserProperties_WithInvalidCorrelationId_DoesNotSetCorrelationId()
        {
            var message = new ReceivedMessage();
            var props = new BasicProperties
            {
                CorrelationId = "not-a-guid",
                Headers = new Dictionary<string, object?> { { "h", "v" } }
            };

            message.PopulateUserProperties(props);

            Assert.That(message.GetCorrelationId(), Is.Null);
        }

        [Test]
        public void PopulateHeaders_NoUserProperties_DoesNotSetHeaders()
        {
            var message = new MessageBusMessage();
            var props = new BasicProperties();

            props.PopulateHeaders(message);

            Assert.That(props.Headers, Is.Null);
        }

        [Test]
        public void PopulateHeaders_WithUserProperties_CopiesThemAsHeaders()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k1", "v1");
            message.AddUserProperty("k2", 2);
            var props = new BasicProperties();

            props.PopulateHeaders(message);

            Assert.That(props.Headers, Is.Not.Null);
            Assert.That(props.Headers!["k1"], Is.EqualTo("v1"));
            Assert.That(props.Headers["k2"], Is.EqualTo(2));
        }

        [Test]
        public void PopulateHeaders_NoMessageId_GeneratesOne()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", "v");
            var props = new BasicProperties();

            props.PopulateHeaders(message);

            Assert.That(props.MessageId, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void PopulateHeaders_MessageIdAlreadySet_DoesNotOverride()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", "v");
            var props = new BasicProperties { MessageId = "already-set" };

            props.PopulateHeaders(message);

            Assert.That(props.MessageId, Is.EqualTo("already-set"));
        }

        [Test]
        public void PopulateHeaders_UsesUserSetMessageId_WhenPresent()
        {
            var message = new MessageBusMessage();
            message.SetMessageId("custom-id");
            var props = new BasicProperties();

            props.PopulateHeaders(message);

            Assert.That(props.MessageId, Is.EqualTo("custom-id"));
        }

        [Test]
        public void CreateReceivedMessage_BuildsMessageWithBodyAndProperties()
        {
            var body = Encoding.UTF8.GetBytes("payload");
            var props = new BasicProperties
            {
                Headers = new Dictionary<string, object?> { { "k", "v" } }
            };

            var message = ((ReadOnlyMemory<byte>)body).CreateReceivedMessage(props);

            Assert.That(message.GetBody(), Is.EqualTo("payload"));
            Assert.That(message.IsJson, Is.True);
            Assert.That(message.UserProperties["k"], Is.EqualTo("v"));
        }

        [Test]
        public void ConvertPropertyValue_ByteArray_DecodesAsUtf8String()
        {
            var result = ReceivedMessageExtensions.ConvertPropertyValue(Encoding.UTF8.GetBytes("hello"));

            Assert.That(result, Is.EqualTo("hello"));
        }

        [Test]
        public void ConvertPropertyValue_NonByteArray_ReturnsAsIs()
        {
            var result = ReceivedMessageExtensions.ConvertPropertyValue(42);

            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void ConvertPropertyValue_Null_ReturnsNull()
        {
            var result = ReceivedMessageExtensions.ConvertPropertyValue(null);

            Assert.That(result, Is.Null);
        }
    }
}
