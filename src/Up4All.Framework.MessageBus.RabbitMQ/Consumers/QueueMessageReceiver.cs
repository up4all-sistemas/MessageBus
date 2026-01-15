using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;

namespace Up4All.Framework.MessageBus.RabbitMQ.Consumers
{
    public class AsyncQueueMessageReceiver(IChannel channel, Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> idleHandler, bool autocomplete)
        : AsyncEventingBasicConsumer(channel)
    {
        private readonly IChannel _channel = channel;
        private readonly Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> _handler = handler;
        private readonly Func<Exception, CancellationToken, Task> _errorHandler = errorHandler;
        private readonly Func<CancellationToken, Task> _idleHandler = idleHandler;
        private readonly bool _autoComplete = autocomplete;

        public override async Task HandleBasicDeliverAsync(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken = default)
        {
            await base.HandleBasicDeliverAsync(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body, cancellationToken: cancellationToken);
            using var activity = this.CreateMessageReceivedActivity(properties, exchange, routingKey);
            try
            {
                var message = body.CreateReceivedMessage(properties.Headers);
                RabbitMQClientExtensions.AddTagsToActivity(activity, exchange, routingKey, body.ToArray());
                var response = await _handler(message, cancellationToken);
                await _channel.ProcessMessageAsync(deliveryTag, response, _autoComplete, cancellationToken);
                await _idleHandler?.Invoke(cancellationToken);
            }
            catch (Exception ex)
            {
                await _channel.ProcessErrorMessageAsync(deliveryTag, _autoComplete, cancellationToken);
                await _errorHandler(ex, CancellationToken.None);
            }
        }
    }

    public class AsyncQueueMessageReceiverForModel<TModel>(IChannel channel, Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> idleHandler, bool autocomplete)
        : AsyncQueueMessageReceiver(channel, (msg, ct) =>
        {
            var model = msg.GetBody<TModel>();
            return handler(model, ct);
        }, errorHandler, idleHandler, autocomplete)
    {
    }
}
