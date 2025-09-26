using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;

namespace Up4All.Framework.MessageBus.RabbitMQ.Consumers
{   



    public class AsyncQueueMessageReceiver(IModel channel, Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, bool autocomplete) 
        : AsyncEventingBasicConsumer(channel)
    {
        private readonly IModel _channel = channel;
        private readonly Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> _handler = handler;
        private readonly Func<Exception, CancellationToken, Task> _errorHandler = errorHandler;
        private readonly bool _autoComplete = autocomplete;

        public override async Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            await base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
            using var activity = this.CreateMessageReceivedActivity(properties, exchange, routingKey);
            try
            {
                var message = body.CreateReceivedMessage(properties.Headers);
                RabbitMQClientExtensions.AddTagsToActivity(activity, exchange, routingKey, body.ToArray());
                var response = await _handler(message, CancellationToken.None);
                _channel.ProcessMessage(deliveryTag, response, _autoComplete);
            }
            catch (Exception ex)
            {
                _channel.ProcessErrorMessage(deliveryTag, _autoComplete);                
                await _errorHandler(ex, CancellationToken.None);
            }
        }
    }

    public class AsyncQueueMessageReceiverForModel<TModel>(IModel channel, Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, bool autocomplete)
        : AsyncQueueMessageReceiver(channel, (msg, ct) => {
                var model = msg.GetBody<TModel>();
                return handler(model, ct);
            }, errorHandler, autocomplete)
    {
    }


    public class QueueMessageReceiver(IModel channel, Func<ReceivedMessage, MessageReceivedStatus> handler, Action<Exception> errorHandler, bool autocomplete)
                : EventingBasicConsumer(channel)
    {
        private readonly IModel _channel = channel;
        private readonly Func<ReceivedMessage, MessageReceivedStatus> _handler = handler;
        private readonly Action<Exception> _errorHandler = errorHandler;
        private readonly bool _autoComplete = autocomplete;

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
            using var activivy = this.CreateMessageReceivedActivity(properties, exchange, routingKey);
            try
            {
                var message = body.CreateReceivedMessage(properties.Headers);
                RabbitMQClientExtensions.AddTagsToActivity(activivy, exchange, routingKey, body.ToArray());
                var response = _handler(message);
                _channel.ProcessMessage(deliveryTag, response, _autoComplete);
            }
            catch (Exception ex)
            {
                _channel.ProcessErrorMessage(deliveryTag, _autoComplete);
                _errorHandler(ex);
            }
        }

    }

    public class QueueMessageReceiverForModel<TModel>(IModel channel, Func<TModel, MessageReceivedStatus> handler, Action<Exception> errorHandler, bool autocomplete)
        : QueueMessageReceiver(channel, (msg) => {
                var model = msg.GetBody<TModel>();
                return handler(model);
            }, errorHandler, autocomplete)
    {
    }
}
