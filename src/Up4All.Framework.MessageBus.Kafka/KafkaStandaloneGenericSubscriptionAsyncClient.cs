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
    public class KafkaStandaloneGenericSubscriptionAsyncClient<TMessageKey> : MessageBusStandaloneSubscriptonClient, IMessageBusStandaloneAsyncConsumer, IKafkaSubscriptionClient
        where TMessageKey : class
    {
        private readonly IConsumer<TMessageKey, byte[]> _consumer;        

        public KafkaStandaloneGenericSubscriptionAsyncClient(string connectionString, string topicName, string subscriptionName)
            : base(connectionString, topicName, subscriptionName)
        {
            _consumer = this.CreateConsumer<TMessageKey>(connectionString, subscriptionName);
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
                    var message = consume.Message.ToReceivedMessage();

                    var result = await handler(message, cancellationToken);

                    if (result == MessageReceivedStatus.Completed)
                        _consumer.Commit();

                    if (onIdle is not null)
                        await onIdle(cancellationToken);
                }
                catch(OperationCanceledException)
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
                    var message = consume.Message.ToReceivedMessage();

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

        protected override void Dispose(bool disposing)
        {
            _consumer.Dispose();
        }
    }

    public class KafkaStandaloneWithStructKeySubscriptionAsyncClient<TMessageKey> : MessageBusStandaloneSubscriptonClient, IMessageBusStandaloneAsyncConsumer, IKafkaSubscriptionClient
        where TMessageKey : struct
    {
        private readonly IConsumer<TMessageKey, byte[]> _consumer;

        public KafkaStandaloneWithStructKeySubscriptionAsyncClient(string connectionString, string topicName, string subscriptionName)
            : base(connectionString, topicName, subscriptionName)
        {
            _consumer = this.CreateConsumerForStructKey<TMessageKey>(connectionString, subscriptionName);
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
                    var message = consume.Message.ToReceivedMessageWithStructKey();

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
                    var message = consume.Message.ToReceivedMessageWithStructKey();

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

        protected override void Dispose(bool disposing)
        {
            _consumer.Dispose();
        }
    }
}
