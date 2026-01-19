using Confluent.Kafka;

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
    public abstract class KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey> : MessageBusStandaloneSubscriptonClient, IMessageBusStandaloneAsyncConsumer, IKafkaSubscriptionClient
    {
        private readonly IConsumer<TMessageKey, byte[]> _consumer;

        public KafkaStandaloneGenericSubscriptionAsyncClient(string connectionString, string topicName, string subscriptionName)
            : base(connectionString, topicName, subscriptionName)
        {
            _consumer = CreateConsumer(connectionString, subscriptionName);
        }

        public Task CloseAsync(CancellationToken cancellationToken = default)
        {
            _consumer.Unsubscribe();
            _consumer.Unassign();
            _consumer.Close();
            return Task.CompletedTask;
        }

        public async Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task>? onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            _consumer.Subscribe(TopicName);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consume = _consumer.Consume(cancellationToken);
                    var message = GetReceivedMessage(consume.Message);

                    this.AddActivityTrace<KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>>(message);

                    var result = await handler(message, cancellationToken);

                    if (result == MessageReceivedStatus.Completed)
                        _consumer.Commit();

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
            _consumer.Subscribe(TopicName);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consume = _consumer.Consume(cancellationToken);
                    var message = GetReceivedMessage(consume.Message);

                    this.AddActivityTrace<KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>>(message);

                    var result = await handler(message.GetBody<TModel>(), cancellationToken);

                    if (result == MessageReceivedStatus.Completed)
                        _consumer.Commit();

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

        protected abstract IConsumer<TMessageKey, byte[]> CreateConsumer(string connectionString, string subscriptionName);

        protected abstract ReceivedMessage GetReceivedMessage(Message<TMessageKey, byte[]> consumeMessage);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _consumer.Dispose();
        }
    }

    public class KafkaStandaloneWithGenericSubscriptionAsyncClient<TMessageKey>(string connectionString, string topicName, string subscriptionName)
        : KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>(connectionString, topicName, subscriptionName)
        , IMessageBusStandaloneAsyncConsumer, IKafkaSubscriptionClient
        where TMessageKey : class
    {
        protected override IConsumer<TMessageKey, byte[]> CreateConsumer(string connectionString, string subscriptionName)
        {
            return new ConsumerBuilder<TMessageKey, byte[]>(this.CreateConfig(connectionString, subscriptionName)).Build();
        }

        protected override ReceivedMessage GetReceivedMessage(Message<TMessageKey, byte[]> consumeMessage)
        {
            return consumeMessage.ToReceivedMessage();
        }
    }

    public class KafkaStandaloneWithStructKeySubscriptionAsyncClient<TMessageKey>(string connectionString, string topicName, string subscriptionName)
        : KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey>(connectionString, topicName, subscriptionName)
        , IMessageBusStandaloneAsyncConsumer, IKafkaSubscriptionClient
        where TMessageKey : struct
    {
        protected override IConsumer<TMessageKey, byte[]> CreateConsumer(string connectionString, string subscriptionName)
        {
            return new ConsumerBuilder<TMessageKey, byte[]>(this.CreateConfig(connectionString, subscriptionName)).Build();
        }

        protected override ReceivedMessage GetReceivedMessage(Message<TMessageKey, byte[]> consumeMessage)
        {
            return consumeMessage.ToReceivedMessageWithStructKey();
        }

    }
}
