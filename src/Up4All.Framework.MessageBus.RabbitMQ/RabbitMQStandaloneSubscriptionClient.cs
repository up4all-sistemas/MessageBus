using RabbitMQ.Client;

using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Consumers;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQStandaloneSubscriptionClient : MessageBusStandaloneSubscribeClient, IRabbitMQClient, IMessageBusStandaloneConsumer, IDisposable
    {
        private IModel _channel;
        private readonly string _subscriptionName;
        private readonly int _connectionAttempts;

        public IConnection Connection { get; set; }

        public RabbitMQStandaloneSubscriptionClient(string connectionString, string subscriptionName, int connectionAttempts = 8) : base(connectionString, "", subscriptionName)
        {
            _subscriptionName = subscriptionName;
            _connectionAttempts = connectionAttempts;
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _channel = this.CreateChannel(this.GetConnection(ConnectionString, _connectionAttempts));
            var receiver = new QueueMessageReceiver(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, _subscriptionName, receiver, autoComplete);
        }

        public override Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            _channel = this.CreateChannel(this.GetConnection(ConnectionString, _connectionAttempts));
            var receiver = new QueueMessageReceiver(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, _subscriptionName, receiver, autoComplete);
            return Task.CompletedTask;
        }

        public override Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            _channel = this.CreateChannel(this.GetConnection(ConnectionString, _connectionAttempts));
            var receiver = new QueueMessageReceiverForModel<TModel>(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, _subscriptionName, receiver, autoComplete);
            return Task.CompletedTask;
        }

        public override void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _channel = this.CreateChannel(this.GetConnection(ConnectionString, _connectionAttempts));
            var receiver = new QueueMessageReceiverForModel<TModel>(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, _subscriptionName, receiver, autoComplete);
        }

        public void Dispose()
        {
            Close();
        }

        public override Task Close()
        {
            _channel?.Close();
            Connection.Close();
            return Task.CompletedTask;
        }


    }
}
