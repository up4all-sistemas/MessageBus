using Confluent.Kafka;
using Confluent.Kafka.Admin;

using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
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
                try
                {
                    var consume = Consumer.Consume(cancellationToken);
                    var message = GetReceivedMessage(consume.Message);

                    this.AddActivityTrace<KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>>(message);

                    var result = await handler(message, cancellationToken);

                    if (result == MessageReceivedStatus.Completed)
                        Consumer.Commit();

                    if (onIdle is not null)
                        await onIdle(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    await CloseAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    await errorHandler(ex, cancellationToken);
                }
            }
        }

        public async Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task>? onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            Consumer.Subscribe(TopicName);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consume = Consumer.Consume(cancellationToken);
                    var message = GetReceivedMessage(consume.Message);

                    this.AddActivityTrace<KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>>(message);

                    var result = await handler(message.GetBody<TModel>(), cancellationToken);

                    if (result == MessageReceivedStatus.Completed)
                        Consumer.Commit();

                    if (onIdle is not null)
                        await onIdle(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    await CloseAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    await errorHandler(ex, cancellationToken);
                }
            }
        }

        protected void SetConsumer(IConsumer<TMessageKey, byte[]> consumer) 
        {
            Consumer = consumer;
        }

        protected abstract ReceivedMessage GetReceivedMessage(Message<TMessageKey, byte[]> consumeMessage);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Consumer.Dispose();
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

        protected override ReceivedMessage GetReceivedMessage(Message<TMessageKey, byte[]> consumeMessage)
        {
            return consumeMessage.ToReceivedMessageWithStructKey();
        }

    }
}
