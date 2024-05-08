using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;

namespace Up4All.Framework.MessageBus.RabbitMQ.Consumers
{
    public class QueueMessageReceiver : EventingBasicConsumer
    {
        private readonly IModel _channel;
        private readonly Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> _handler;
        private readonly Func<Exception, CancellationToken, Task> _errorHandler;
        private readonly bool _autoComplete;

        public QueueMessageReceiver(IModel channel, Func<ReceivedMessage, MessageReceivedStatus> handler, Action<Exception> errorHandler, bool autocomplete) : base(channel)
        {
            _autoComplete = autocomplete;
            _channel = channel;
            _handler = (msg, c) =>
            {
                c.ThrowIfCancellationRequested();
                return Task.FromResult(handler(msg));
            };
            _errorHandler = (ex, c) =>
            {
                c.ThrowIfCancellationRequested();
                errorHandler(ex);
                return Task.CompletedTask;
            };
        }

        public QueueMessageReceiver(IModel channel, Func<ReceivedMessage, Task<MessageReceivedStatus>> handler, Func<Exception, Task> errorHandler, bool autocomplete) : base(channel)
        {
            _autoComplete = autocomplete;
            _channel = channel;
            _handler = (msg, c) => { c.ThrowIfCancellationRequested(); return handler(msg); };
            _errorHandler = (ex, c) => { c.ThrowIfCancellationRequested(); return errorHandler(ex); };
        }

        public QueueMessageReceiver(IModel channel, Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, bool autocomplete) : base(channel)
        {
            _autoComplete = autocomplete;
            _channel = channel;
            _handler = handler;
            _errorHandler = errorHandler;
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            try
            {
                var message = new ReceivedMessage();
                message.AddBody(BinaryData.FromBytes(body), true);
                message.PopulateUserProperties(properties.Headers);

                var response = _handler(message, CancellationToken.None).GetAwaiter().GetResult();

                if (!_autoComplete && response == MessageReceivedStatus.Deadletter)
                {
                    _channel.BasicNack(deliveryTag, false, false);
                    return;
                }

                if (!_autoComplete && response == MessageReceivedStatus.Abandoned)
                {
                    _channel.BasicReject(deliveryTag, true);
                    return;
                }

                if (!_autoComplete)
                    _channel.BasicAck(deliveryTag, false);
            }
            catch (Exception ex)
            {
                if (!_autoComplete)
                    _channel.BasicNack(deliveryTag, false, false);
                _errorHandler(ex, CancellationToken.None).Wait();
            }
        }
    }

    public class QueueMessageReceiverForModel<TModel> : DefaultBasicConsumer
    {
        private readonly IModel _channel;
        private readonly Func<TModel, CancellationToken, Task<MessageReceivedStatus>> _handler;
        private readonly Func<Exception, CancellationToken, Task> _errorHandler;
        private readonly bool _autoComplete;

        public QueueMessageReceiverForModel(IModel channel, Func<TModel, MessageReceivedStatus> handler, Action<Exception> errorHandler, bool autocomplete)
        {
            _autoComplete = autocomplete;
            _channel = channel;
            _handler = (msg, c) =>
            {
                c.ThrowIfCancellationRequested();
                return Task.FromResult(handler(msg));
            };
            _errorHandler = (ex, c) =>
            {
                c.ThrowIfCancellationRequested();
                errorHandler(ex);
                return Task.CompletedTask;
            };
        }

        public QueueMessageReceiverForModel(IModel channel, Func<TModel, Task<MessageReceivedStatus>> handler, Func<Exception, Task> errorHandler, bool autocomplete)
        {
            _autoComplete = autocomplete;
            _channel = channel;
            _handler = (msg, c) => { c.ThrowIfCancellationRequested(); return handler(msg); };
            _errorHandler = (ex, c) => { c.ThrowIfCancellationRequested(); return errorHandler(ex); };
        }

        public QueueMessageReceiverForModel(IModel channel, Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, bool autocomplete)
        {
            _autoComplete = autocomplete;
            _channel = channel;
            _handler = handler;
            _errorHandler = errorHandler;
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            try
            {
                var model = JsonSerializer.Deserialize<TModel>(body.ToArray(), new JsonSerializerOptions(JsonSerializerDefaults.Web));

                var response = _handler(model, CancellationToken.None).GetAwaiter().GetResult();

                if (!_autoComplete && response == MessageReceivedStatus.Deadletter)
                {
                    _channel.BasicNack(deliveryTag, false, false);
                    return;
                }

                if (!_autoComplete && response == MessageReceivedStatus.Abandoned)
                {
                    _channel.BasicReject(deliveryTag, true);
                    return;
                }

                if (!_autoComplete)
                    _channel.BasicAck(deliveryTag, false);
            }
            catch (Exception ex)
            {
                if (!_autoComplete)
                    _channel.BasicNack(deliveryTag, false, false);
                _errorHandler(ex, CancellationToken.None).Wait();
            }
        }


    }
}
