using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;

namespace Up4All.Framework.MessageBus.RabbitMQ.Consumers
{
    public class AsyncQueueMessageReceiver(IChannel channel, Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler
        , Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task>? idleHandler, bool autocomplete
        , ILogger logger)
        : AsyncEventingBasicConsumer(channel)
    {
        private readonly IChannel _channel = channel;
        private readonly Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> _handler = handler;
        private readonly Func<Exception, CancellationToken, Task> _errorHandler = errorHandler;
        private readonly Func<CancellationToken, Task>? _idleHandler = idleHandler;
        private readonly bool _autoComplete = autocomplete;
        private readonly ILogger _logger = logger;

        public override async Task HandleBasicDeliverAsync(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Registrating Deliver Consumer Async");
            await base.HandleBasicDeliverAsync(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body, cancellationToken: cancellationToken);

            Activity? activity = null;
            var entityPath = $"{exchange}:{_channel.CurrentQueue}";
            var stopwatch = Stopwatch.StartNew();
            string? errorType = null;
            try
            {
                _logger.LogDebug("Receiving message from {QueueName}", _channel.CurrentQueue);
                var message = body.CreateReceivedMessage(properties);
                if (properties.MessageId is not null)
                    message.SetMessageId(properties.MessageId);

                var additionaArgs = new Dictionary<string, object> {
                    { "messaging.rabbitmq.message.delivery_tag", deliveryTag }
                };

                if (!string.IsNullOrEmpty(routingKey))
                    additionaArgs.Add("messaging.rabbitmq.destination.routing_key", routingKey);

                activity = this.CreateMessageReceivedActivity(properties, exchange, routingKey);
                activity?.InjectPropagationContext(message.UserProperties);
                activity?.AddTagsToActivity("rabbitmq", message, entityPath, properties.MessageId, additionalTags: additionaArgs);

                var response = await _handler(message, cancellationToken);
                await _channel.ProcessMessageAsync(deliveryTag, response, _autoComplete, cancellationToken);
                activity?.SetStatus(ActivityStatusCode.Ok);
                RabbitMQClientExtensions.ConsumedMessagesCounter.RecordMessageConsumed("rabbitmq", entityPath);

                if (_idleHandler is not null)
                    await _idleHandler.Invoke(cancellationToken);
            }
            catch (Exception ex)
            {
                errorType = ex.GetType().Name;
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
                _logger.LogError(ex, "Receiver Error");
                await _channel.ProcessErrorMessageAsync(deliveryTag, _autoComplete, cancellationToken);
                RabbitMQClientExtensions.ConsumedMessagesCounter.RecordMessageConsumed("rabbitmq", entityPath, errorType);
                await _errorHandler(ex, CancellationToken.None);
            }
            finally
            {
                RabbitMQClientExtensions.OperationDurationHistogram.RecordOperationDuration(stopwatch.Elapsed.TotalSeconds, "rabbitmq", entityPath, "receive", errorType);
                activity?.Dispose();
            }
        }
    }

    public class AsyncQueueMessageReceiverForModel<TModel>(IChannel channel, Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler
        , Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task>? idleHandler, bool autocomplete
        , ILogger logger)
        : AsyncQueueMessageReceiver(channel, (msg, ct) =>
        {
            var model = msg.GetBody<TModel>()!;
            return handler(model, ct);
        }, errorHandler, idleHandler, autocomplete, logger)
    {
    }
}
