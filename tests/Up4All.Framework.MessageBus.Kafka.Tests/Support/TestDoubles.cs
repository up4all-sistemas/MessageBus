using Confluent.Kafka;

using System.Reflection;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Kafka.Tests.Support
{
    internal class FakeStandaloneSubscriptonClient(string connectionString, string topicName, string subscriptionName)
        : MessageBusStandaloneSubscriptonClient(connectionString, topicName, subscriptionName)
    {
        protected override void Dispose(bool disposing) { }
    }

    internal class FakeStandaloneTopicClient(string connectionString, string topicName, int connectionAttempts = 8)
        : MessageBusStandaloneTopicClient(connectionString, topicName, connectionAttempts)
    {
        protected override void Dispose(bool disposing) { }
    }

    /// <summary>
    /// KafkaStandaloneGenericSubscriptionAsyncClient's own constructor never builds a real
    /// Confluent.Kafka IConsumer (only the concrete With*Key subclasses do, in their own
    /// constructors), so subclassing it directly and calling SetConsumerForTest lets tests
    /// fully control consumption without ever touching the network.
    /// </summary>
    internal class TestableKafkaSubscriptionClient(string connectionString, string topicName, string subscriptionName)
        : KafkaStandaloneGenericSubscriptionAsyncClient<string>(connectionString, topicName, subscriptionName)
    {
        public void SetConsumerForTest(IConsumer<string, byte[]> consumer) => SetConsumer(consumer);

        protected override ReceivedMessage GetReceivedMessage(Message<string, byte[]> consumeMessage)
        {
            var result = new ReceivedMessage();
            result.SetMessageId(consumeMessage.Key ?? string.Empty);
            result.AddBody(consumeMessage.Value);
            return result;
        }

        protected override object? GetMessageId(MessageBusMessage message)
            => message.GetMessageIdForClass<string>();
    }

    internal static class KafkaTestHelpers
    {
        /// <summary>
        /// KafkaStandaloneGenericTopicAsyncClient/KafkaStandaloneWithStructKeyTopicAsyncClient
        /// build a real IProducer directly inside their constructor (no injection point), so
        /// this disposes that real-but-unused producer and swaps in a mock via reflection on
        /// the private "_producer" field, avoiding any real broker I/O for SendAsync tests.
        /// </summary>
        public static void ReplaceProducer(object client, object mockProducer)
        {
            FieldInfo? field = null;
            for (var type = client.GetType(); type is not null && field is null; type = type.BaseType)
                field = type.GetField("_producer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (field is null)
                throw new System.InvalidOperationException("_producer field not found");

            if (field.GetValue(client) is System.IDisposable existing)
                existing.Dispose();

            field.SetValue(client, mockProducer);
        }
    }
}
