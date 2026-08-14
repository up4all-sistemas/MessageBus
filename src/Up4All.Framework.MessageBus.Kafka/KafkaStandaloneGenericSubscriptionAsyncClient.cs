using Confluent.Kafka;
using Confluent.Kafka.Admin;

using OpenTelemetry.Trace;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Kafka.Extensions;
using Up4All.Framework.MessageBus.Kafka.Interfaces;

namespace Up4All.Framework.MessageBus.Kafka
{
    public abstract class KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>(string connectionString, string topicName, string subscriptionName)
        : MessageBusStandaloneSubscriptonClient(connectionString, topicName, subscriptionName)
        , IMessageBusStandaloneAsyncConsumer, IKafkaSubscriptionClient
    {
        protected IConsumer<TMessageKey, byte[]> Consumer { get; private set; } = null!;

        public Task CloseAsync(CancellationToken cancellationToken = default)
        {
            Consumer.Unsubscribe();
            Consumer.Unassign();
            Consumer.Close();
            return Task.CompletedTask;
        }

        public async Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task>? onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            Consumer.Subscribe(TopicName);
            while (!cancellationToken.IsCancellationRequested)
            {
                Activity? activity = null;
                var stopwatch = Stopwatch.StartNew();
                string? errorType = null;
                try
                {
                    var consume = Consumer.Consume(cancellationToken);
                    var message = GetReceivedMessage(consume.Message);

                    activity = this.AddActivityTrace<KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>>(message, GetMessageId(message)
                        , BuildKafkaActivityTags(consume));

                    var result = await handler(message, cancellationToken);

                    if (result == MessageReceivedStatus.Completed)
                        Consumer.Commit();

                    activity?.SetStatus(ActivityStatusCode.Ok);
                    KafkaExtensions.ConsumedMessagesCounter.RecordMessageConsumed("kafka", EntityPath);

                    if (onIdle is not null)
                        await onIdle(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    await CloseAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    errorType = ex.GetType().Name;
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddException(ex);
                    KafkaExtensions.ConsumedMessagesCounter.RecordMessageConsumed("kafka", EntityPath, errorType);
                    await errorHandler(ex, cancellationToken);
                }
                finally
                {
                    KafkaExtensions.OperationDurationHistogram.RecordOperationDuration(stopwatch.Elapsed.TotalSeconds, "kafka", EntityPath, "receive", errorType);
                    activity?.Dispose();
                }
            }
        }

        public async Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task>? onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            Consumer.Subscribe(TopicName);
            while (!cancellationToken.IsCancellationRequested)
            {
                Activity? activity = null;
                var stopwatch = Stopwatch.StartNew();
                string? errorType = null;
                try
                {
                    var consume = Consumer.Consume(cancellationToken);
                    var message = GetReceivedMessage(consume.Message);

                    activity = this.AddActivityTrace<KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>>(message, GetMessageId(message)
                        , BuildKafkaActivityTags(consume));

                    var result = await handler(message.GetBody<TModel>()!, cancellationToken);

                    if (result == MessageReceivedStatus.Completed)
                        Consumer.Commit();

                    activity?.SetStatus(ActivityStatusCode.Ok);
                    KafkaExtensions.ConsumedMessagesCounter.RecordMessageConsumed("kafka", EntityPath);

                    if (onIdle is not null)
                        await onIdle(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    await CloseAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    errorType = ex.GetType().Name;
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddException(ex);
                    KafkaExtensions.ConsumedMessagesCounter.RecordMessageConsumed("kafka", EntityPath, errorType);
                    await errorHandler(ex, cancellationToken);
                }
                finally
                {
                    KafkaExtensions.OperationDurationHistogram.RecordOperationDuration(stopwatch.Elapsed.TotalSeconds, "kafka", EntityPath, "receive", errorType);
                    activity?.Dispose();
                }
            }
        }

        protected void SetConsumer(IConsumer<TMessageKey, byte[]> consumer)
        {
            Consumer = consumer;
        }

        // Kafka-specific attributes the OpenTelemetry messaging semantic conventions define
        // for consume spans (mirroring the delivery_tag/routing_key tags RabbitMQ already
        // adds, and the delivery_count/enqueued_time tags Service Bus already adds).
        private Dictionary<string, object> BuildKafkaActivityTags(ConsumeResult<TMessageKey, byte[]> consume)
        {
            var tags = new Dictionary<string, object>
            {
                { "messaging.kafka.message.partition", consume.Partition.Value },
                { "messaging.kafka.message.offset", consume.Offset.Value },
                { "messaging.kafka.consumer.group", EntityPath }
            };

            if (consume.Message.Key is not null)
                tags["messaging.kafka.message.key"] = consume.Message.Key;

            return tags;
        }

        protected abstract ReceivedMessage GetReceivedMessage(Message<TMessageKey, byte[]> consumeMessage);

        protected abstract object? GetMessageId(MessageBusMessage message);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Consumer.Dispose();
        }

        protected override ValueTask DisposeAsyncCore()
        {
            // IConsumer<,> only implements IDisposable (Confluent.Kafka has no async
            // close/dispose API). Offloading to the thread pool keeps DisposeAsync() from
            // blocking whichever thread is driving the async disposal chain.
            return new ValueTask(Task.Run(() => Dispose(true)));
        }
    }

    public class KafkaStandaloneWithGenericSubscriptionAsyncClient<TMessageKey>
        : KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>
        , IMessageBusStandaloneAsyncConsumer, IKafkaSubscriptionClient
        where TMessageKey : class
    {
        public KafkaStandaloneWithGenericSubscriptionAsyncClient(string connectionString, string topicName, string subscriptionName)
            : base(connectionString, topicName, subscriptionName)
        {
            SetConsumer(new ConsumerBuilder<TMessageKey, byte[]>(this.CreateConfig(connectionString, subscriptionName)).Build());
        }

        protected override object? GetMessageId(MessageBusMessage message)
        {
            return message.GetMessageIdForClass<TMessageKey>();
        }

        protected override ReceivedMessage GetReceivedMessage(Message<TMessageKey, byte[]> consumeMessage)
        {
            return consumeMessage.ToReceivedMessage();
        }
    }

    public class KafkaStandaloneWithStructKeySubscriptionAsyncClient<TMessageKey>
        : KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>
        , IMessageBusStandaloneAsyncConsumer, IKafkaSubscriptionClient
        where TMessageKey : struct
    {
        public KafkaStandaloneWithStructKeySubscriptionAsyncClient(string connectionString, string topicName, string subscriptionName)
            : base(connectionString, topicName, subscriptionName)
        {
            SetConsumer(new ConsumerBuilder<TMessageKey, byte[]>(this.CreateConfig(connectionString, subscriptionName)).Build());
        }

        protected override object? GetMessageId(MessageBusMessage message)
        {
            return message.GetMessageIdForStruct<TMessageKey>();
        }

        protected override ReceivedMessage GetReceivedMessage(Message<TMessageKey, byte[]> consumeMessage)
        {
            return consumeMessage.ToReceivedMessageWithStructKey();
        }

    }
}
