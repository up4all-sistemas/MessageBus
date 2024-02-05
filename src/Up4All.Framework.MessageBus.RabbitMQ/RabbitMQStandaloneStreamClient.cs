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

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQStandaloneStreamClient : MessageBusStandaloneStreamClient, IRabbitMQClient, IMessageBusStandaloneStreamClient, IDisposable
    {
        private readonly IModel _channel;
        private readonly string _streamname;
        private readonly int _connectionAttempts;

        public IConnection Connection { get; set; }

        public RabbitMQStandaloneStreamClient(string connectionString, string streamname, object offset, int connectionAttempts = 8
            , bool exclusive = false, bool durable = true, bool autoDelete = false, Dictionary<string, object> args = null) : base(connectionString, streamname, offset)
        {
            _streamname = streamname;
            _connectionAttempts = connectionAttempts;
            _channel = this.CreateChannel(this.GetConnection(ConnectionString, _connectionAttempts));

            if (args == null)
                args = new Dictionary<string, object>();

            if (!args.ContainsKey("x-stream-type"))
                args.Add("x-stream-type", "stream");

            _channel.QueueDeclare(streamname, durable, exclusive, autoDelete, args);
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {

            var receiver = new QueueMessageReceiver(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, _streamname, receiver, false, Offset);
        }

        public override Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {

            var receiver = new QueueMessageReceiver(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, _streamname, receiver, false, Offset);
            return Task.CompletedTask;
        }

        public override Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {

            var receiver = new QueueMessageReceiverForModel<TModel>(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, _streamname, receiver, false, Offset);
            return Task.CompletedTask;
        }

        public override void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            var receiver = new QueueMessageReceiverForModel<TModel>(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, _streamname, receiver, false, Offset);
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

        public override async Task SendAsync<TModel>(TModel model, CancellationToken cancellation = default)
        {
            var message = model.CreateMessagebusMessage();
            await SendAsync(message, cancellation);
        }

        public override async Task SendManyAsync<TModel>(IEnumerable<TModel> list, CancellationToken cancellation = default)
        {
            await SendAsync(list.Select(x => x.CreateMessagebusMessage()), cancellation);
        }

        public override Task SendAsync(MessageBusMessage message, CancellationToken cancellation = default)
        {
            using (var conn = this.GetConnection(ConnectionString, _connectionAttempts))
            {
                using (var channel = this.CreateChannel(conn))
                {
                    channel.SendMessage("", _streamname, message, cancellation);
                }
            }

            return Task.CompletedTask;
        }

        public override Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellation = default)
        {
            using (var conn = this.GetConnection(ConnectionString, _connectionAttempts))
            {
                using (var channel = this.CreateChannel(conn))
                {
                    foreach (var message in messages)
                        channel.SendMessage("", _streamname, message, cancellation);
                }
            }

            return Task.CompletedTask;
        }

    }
}
