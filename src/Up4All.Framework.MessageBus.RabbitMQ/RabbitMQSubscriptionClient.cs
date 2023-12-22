using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;

using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Consumers;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQSubscriptionClient : MessageBusSubscribeClient, IRabbitMQClient, IMessageBusConsumer, IDisposable
    {
        private readonly object _offset;
        private readonly ILogger<RabbitMQSubscriptionClient> _logger;
        private IModel _channel;

        public IConnection Connection { get; set; }

        public RabbitMQSubscriptionClient(ILogger<RabbitMQSubscriptionClient> logger, IOptions<MessageBusOptions> messageOptions, object offset = null) : base(messageOptions)
        {
            _logger = logger;
            _offset = offset;
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _channel = this.CreateChannel(this.GetConnection(MessageBusOptions, _logger));
            var receiver = new QueueMessageReceiver(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, MessageBusOptions.SubscriptionName, receiver, autoComplete, _offset);
        }

        public override Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            _channel = this.CreateChannel(this.GetConnection(MessageBusOptions, _logger));
            var receiver = new QueueMessageReceiver(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, MessageBusOptions.SubscriptionName, receiver, autoComplete, _offset);
            return Task.CompletedTask;
        }

        public override Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            _channel = this.CreateChannel(this.GetConnection(MessageBusOptions, _logger));
            var receiver = new QueueMessageReceiverForModel<TModel>(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, MessageBusOptions.SubscriptionName, receiver, autoComplete, _offset);
            return Task.CompletedTask;
        }

        public override void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _channel = this.CreateChannel(this.GetConnection(MessageBusOptions, _logger));
            var receiver = new QueueMessageReceiverForModel<TModel>(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, MessageBusOptions.SubscriptionName, receiver, autoComplete, _offset);
        }

        public void Dispose()
        {
            Close();
        }

        public override Task Close()
        {
            _channel?.Close();
            this.Connection?.Close();
            return Task.CompletedTask;
        }


    }
}
