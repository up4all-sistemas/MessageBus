using Confluent.Kafka;

using System.Diagnostics;
using System.Text.Json;

using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Kafka.Consumers;
using Up4All.Framework.MessageBus.Kafka.Interfaces;

namespace Up4All.Framework.MessageBus.Kafka.Extensions
{
    public static class KafkaExtensions
    {
        public static ActivitySource ActivitySource => OpenTelemetryExtensions.CreateActivitySource<KafkaMQDefaultConsumer>();

        public static IProducer<TMessageKey, byte[]> CreateProducer<TMessageKey>(this IKafkaTopicClient client, string connectionString)
            where TMessageKey : class
        {
            var config = new ProducerConfig { BootstrapServers = connectionString };
            return new ProducerBuilder<TMessageKey, byte[]>(config).Build();
        }

        public static IProducer<TMessageKey, byte[]> CreateProducerForStructKey<TMessageKey>(this IKafkaTopicClient client, string connectionString)
            where TMessageKey : struct
        {
            var config = new ProducerConfig { BootstrapServers = connectionString };
            return new ProducerBuilder<TMessageKey, byte[]>(config).Build();
        }

        public static IConsumer<TMessageKey, byte[]> CreateConsumer<TMessageKey>(this IKafkaSubscriptionClient client, string connectionString, string groupId)
            where TMessageKey : class
        {
            var config = new ConsumerConfig { BootstrapServers = connectionString, GroupId = groupId, AutoOffsetReset = AutoOffsetReset.Earliest };
            return new ConsumerBuilder<TMessageKey, byte[]>(config).Build();
        }

        public static IConsumer<TMessageKey, byte[]> CreateConsumerForStructKey<TMessageKey>(this IKafkaSubscriptionClient client, string connectionString, string groupId)
            where TMessageKey : struct
        {
            var config = new ConsumerConfig { BootstrapServers = connectionString, GroupId = groupId, AutoOffsetReset = AutoOffsetReset.Earliest };
            return new ConsumerBuilder<TMessageKey, byte[]>(config).Build();
        }

        public static ReceivedMessage ToReceivedMessage<TMessageKey>(this Message<TMessageKey, byte[]> message)
            where TMessageKey : class
        {
            var result = new ReceivedMessage();
            result.SetMessageId(message.Key);
            result.AddBody(message.Value);

            foreach (var header in message.Headers)
                result.AddUserProperty(header.Key, JsonSerializer.Deserialize<object>(header.GetValueBytes()));

            return result;
        }

        public static ReceivedMessage ToReceivedMessageWithStructKey<TMessageKey>(this Message<TMessageKey, byte[]> message)
            where TMessageKey : struct
        {
            var result = new ReceivedMessage();
            result.SetMessageIdFromStruct(message.Key);
            result.AddBody(message.Value);

            foreach (var header in message.Headers)
                result.AddUserProperty(header.Key, JsonSerializer.Deserialize<object>(header.GetValueBytes()));

            return result;
        }

        
    }
}
