using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;

namespace Up4All.Framework.MessageBus.RabbitMQ.Consumers
{
    public class AsyncQueueMessageReceiver(IChannel channel, Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler
        , Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> idleHandler, bool autocomplete
        , ILogger logger)
        : AsyncEventingBasicConsumer(channel)
    {
        private readonly IChannel _channel = channel;
        private readonly Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> _handler = handler;
        private readonly Func<Exception, CancellationToken, Task> _errorHandler = errorHandler;
        private readonly Func<CancellationToken, Task> _idleHandler = idleHandler;
        private readonly bool _autoComplete = autocomplete;
        private readonly ILogger _logger = logger;

        public override async Task HandleBasicDeliverAsync(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Registrating Deliver Consumer Async");
            await base.HandleBasicDeliverAsync(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body, cancellationToken: cancellationToken);
            
            try
            {
                _logger.LogDebug("Receiving message from {QueueName}", _channel.CurrentQueue);
                var message = body.CreateReceivedMessage(properties);
                message.SetMessageId(Guid.NewGuid());

                using var activity = this.CreateMessageReceivedActivity(properties, exchange, routingKey);
                activity?.InjectPropagationContext(message.UserProperties);
                activity?.AddTagsToActivity("rabbitmq", body.ToArray(), exchange, new Dictionary<string, object> { { "messaging.rabbitmq.routing_key", "" } });
                
                var response = await _handler(message, cancellationToken);
                await _channel.ProcessMessageAsync(deliveryTag, response, _autoComplete, cancellationToken);
                await _idleHandler?.Invoke(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Receiver Error: {Message}", ex.Message);
                await _channel.ProcessErrorMessageAsync(deliveryTag, _autoComplete, cancellationToken);
                await _errorHandler(ex, CancellationToken.None);
            }
        }
    }

    public class AsyncQueueMessageReceiverForModel<TModel>(IChannel channel, Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler
        , Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> idleHandler, bool autocomplete
        , ILogger logger)
        : AsyncQueueMessageReceiver(channel, (msg, ct) =>
        {
            var model = msg.GetBody<TModel>();
            return handler(model, ct);
        }, errorHandler, idleHandler, autocomplete, logger)
    {
    }
}
