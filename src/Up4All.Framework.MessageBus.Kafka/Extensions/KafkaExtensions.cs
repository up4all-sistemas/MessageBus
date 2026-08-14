using Confluent.Kafka;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;

using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Kafka.Interfaces;

namespace Up4All.Framework.MessageBus.Kafka.Extensions
{
    public static class KafkaExtensions
    {
        // ActivitySource is meant to be treated as a singleton per instrumentation library
        // (see the .NET/OpenTelemetry guidance): a property here would build a brand-new
        // instance - with its own assembly-name/version lookup - on every single publish or
        // consume call. This is also the single ActivitySource for the whole project - there
        // used to be a second, unused one (KafkaConsts.ActivitySource) that was never
        // registered with any TracerProvider nor referenced when creating spans.
        public static readonly ActivitySource ActivitySource = OpenTelemetryExtensions.CreateActivitySource<KafkaStandaloneTopicAsyncClient>();

        // Same singleton-per-instrumentation-library reasoning as ActivitySource above,
        // applied to the Meter and the instruments built on top of it.
        public static readonly Meter Meter = OpenTelemetryMetricsExtensions.CreateMeter<KafkaStandaloneTopicAsyncClient>();

        public static readonly Counter<long> SentMessagesCounter = Meter.CreateCounter<long>(
            OpenTelemetryMetricsExtensions.SentMessagesInstrumentName, unit: "{message}", description: "Number of messages sent to Kafka.");

        public static readonly Counter<long> ConsumedMessagesCounter = Meter.CreateCounter<long>(
            OpenTelemetryMetricsExtensions.ConsumedMessagesInstrumentName, unit: "{message}", description: "Number of messages consumed from Kafka.");

        public static readonly Histogram<double> OperationDurationHistogram = Meter.CreateHistogram<double>(
            OpenTelemetryMetricsExtensions.OperationDurationInstrumentName, unit: "s", description: "Duration of Kafka publish/consume operations.");

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

        public static ConsumerConfig CreateConfig(this IKafkaSubscriptionClient client, string connectionString, string subscriptionName)
        {
            return new ConsumerConfig { BootstrapServers = connectionString, GroupId = subscriptionName, AutoOffsetReset = AutoOffsetReset.Earliest };
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
            result.SetMessageId(message.Key!);
            result.AddBody(message.Value);

            if (message.Headers != null)
            {
                foreach (var header in message.Headers)
                {
                    var val = JsonSerializer.Deserialize<object>(header.GetValueBytes());
                    if (val != null)
                        result.AddUserProperty(header.Key, val);
                }
            }

            return result;
        }

        public static ReceivedMessage ToReceivedMessageWithStructKey<TMessageKey>(this Message<TMessageKey, byte[]> message)
            where TMessageKey : struct
        {
            var result = new ReceivedMessage();
            result.SetMessageIdFromStruct(message.Key);
            result.AddBody(message.Value);

            if (message.Headers != null)
            {
                foreach (var header in message.Headers)
                {
                    var val = JsonSerializer.Deserialize<object>(header.GetValueBytes());
                    if (val != null)
                        result.AddUserProperty(header.Key, val);
                }
            }

            return result;
        }

    }
}
