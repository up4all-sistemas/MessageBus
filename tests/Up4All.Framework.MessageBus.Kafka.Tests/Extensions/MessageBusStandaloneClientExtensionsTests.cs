using System.Diagnostics;
using System.Linq;

using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Kafka.Extensions;
using Up4All.Framework.MessageBus.Kafka.Tests.Support;

namespace Up4All.Framework.MessageBus.Kafka.Tests.Extensions
{
    [TestFixture]
    public class MessageBusStandaloneSubscriptonClientExtensionsTests
    {
        [Test]
        public void AddActivityTrace_ReturnsActivityNamedAfterEntityPath()
        {
            var client = new FakeStandaloneSubscriptonClient("localhost:1", "topic-1", "sub-1");
            var message = new ReceivedMessage();
            message.AddBody("payload");

            using var activity = client.AddActivityTrace<MessageBusStandaloneSubscriptonClientExtensionsTests>(message, "msg-id");

            Assert.That(activity, Is.Not.Null);
            Assert.That(activity!.OperationName, Is.EqualTo("sub-1 receive"));
        }

        [Test]
        public void AddActivityTrace_SetsExpectedTags()
        {
            var client = new FakeStandaloneSubscriptonClient("localhost:1", "topic-1", "sub-1");
            var message = new ReceivedMessage();
            message.AddBody("payload");

            using var activity = client.AddActivityTrace<MessageBusStandaloneSubscriptonClientExtensionsTests>(message, "msg-id");

            Assert.That(activity!.GetTagItem("messaging.system"), Is.EqualTo("kafka"));
            Assert.That(activity.GetTagItem("messaging.destination.name"), Is.EqualTo("sub-1"));
            Assert.That(activity.GetTagItem("messaging.message.id"), Is.EqualTo("msg-id"));
        }

        [Test]
        public void CreateMessageReceivedActivity_UsesEntityPathInName()
        {
            var message = new ReceivedMessage();
            message.AddBody("payload");

            using var activity = message.CreateMessageReceivedActivity<MessageBusStandaloneSubscriptonClientExtensionsTests>("my-entity");

            Assert.That(activity, Is.Not.Null);
            Assert.That(activity!.OperationName, Is.EqualTo("my-entity receive"));
        }
    }

    [TestFixture]
    public class MessageBusStandaloneTopicClientExtensionsTests
    {
        [Test]
        public void AddActivityTrace_DoesNotThrow()
        {
            var client = new FakeStandaloneTopicClient("localhost:1", "topic-1");
            var message = new MessageBusMessage();
            message.AddBody("payload");

            Assert.DoesNotThrow(() => client.AddActivityTrace(message, "msg-id"));
        }

        [Test]
        public void AddActivityTrace_TagsActivityWithPublishOperationType()
        {
            // "publish" is the value defined by the OpenTelemetry messaging semantic
            // conventions for a producer-side operation; "send" is not a recognized value.
            Activity? stopped = null;
            using var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
                ActivityStopped = a => stopped = a
            };
            ActivitySource.AddActivityListener(listener);

            var client = new FakeStandaloneTopicClient("localhost:1", "topic-1");
            var message = new MessageBusMessage();
            message.AddBody("payload");

            client.AddActivityTrace(message, "msg-id");

            Assert.That(stopped, Is.Not.Null);
            Assert.That(stopped!.GetTagItem("messaging.operation.type"), Is.EqualTo("publish"));
        }

        [Test]
        public void AddActivityTrace_NullMessageId_DoesNotThrow()
        {
            var client = new FakeStandaloneTopicClient("localhost:1", "topic-1");
            var message = new MessageBusMessage();
            message.AddBody("payload");

            Assert.DoesNotThrow(() => client.AddActivityTrace(message, null));
        }
    }
}
