using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Consumers;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQStandaloneQueueClient : MessageBusStandaloneQueueClient, IRabbitMQClient, IMessageBusStandaloneQueueClient
    {
        private readonly string _queuename;

        public IConnection Connection { get; set; }
        public IModel Channel { get; private set; }

        public RabbitMQStandaloneQueueClient(string connectionString, string queuename, int connectionAttempts = 8
            , QueueDeclareOptions declareOpts = null)
            : base(connectionString, queuename)
        {
            _queuename = queuename;
            this.GetConnection(ConnectionString, connectionAttempts);
            Channel = this.CreateChannel();
            Channel.ConfigureQueueDeclare(queuename, declareOpts);
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            var receiver = new QueueMessageReceiver(Channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_queuename, receiver, autoComplete);
        }
        public override void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            var receiver = new QueueMessageReceiverForModel<TModel>(Channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_queuename, receiver, autoComplete);
        }
        public override Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {

            var receiver = new QueueMessageReceiver(Channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_queuename, receiver, autoComplete);
            return Task.CompletedTask;
        }
        public override Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {

            var receiver = new QueueMessageReceiverForModel<TModel>(Channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_queuename, receiver, autoComplete);
            return Task.CompletedTask;
        }
        public override async Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            var message = model.CreateMessagebusMessage();
            await SendAsync(message, cancellationToken);
        }        
        public override Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            Channel.SendMessage("", _queuename, message, cancellationToken);
            return Task.CompletedTask;
        }
        public override Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            foreach (var message in messages)
                Channel.SendMessage("", _queuename, message, cancellationToken);

            return Task.CompletedTask;
        }
        public override async Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
        {
            await SendAsync(models.Select(x => x.CreateMessagebusMessage()), cancellationToken);
        }
        public override Task Close()
        {
            Channel?.Close();
            Connection.Close();
            return Task.CompletedTask;
        }
        protected override void Dispose(bool disposing)
        {
            Close();
        }
    }
}
