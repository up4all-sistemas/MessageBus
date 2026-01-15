using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Consumers;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQStandaloneStreamAsyncClient : MessageBusStandaloneStreamClient, IRabbitMQClient, IMessageBusStandaloneAsyncConsumer
    {
        private readonly string _streamname;
        private readonly StreamDeclareOptions _declareopts;

        public IConnection Connection { get; set; }
        public IChannel Channel { get; private set; }

        public RabbitMQStandaloneStreamAsyncClient(string connectionString, string streamname, object offset, int connectionAttempts = 8
            , StreamDeclareOptions declareOpts = null)
            : base(connectionString, streamname, offset, connectionAttempts)
        {
            _streamname = streamname;
            _declareopts = declareOpts;
        }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await this.GetConnectionAsync(ConnectionString, ConnectionAttempts, cancellationToken);

            if (Channel is not null) return;

            Channel = await this.CreateChannelAsync(cancellationToken);
            await Channel.ConfigureQueueDeclareAsync(_streamname, _declareopts, cancellationToken);
        }

        public async Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            await InitializeAsync(cancellationToken);
            var receiver = new AsyncQueueMessageReceiver(Channel, handler, errorHandler, onIdle, autoComplete);
            await this.ConfigureAsyncHandler(_streamname, receiver, false, cancellationToken, Offset);
        }
        public async Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            await InitializeAsync(cancellationToken);
            var receiver = new AsyncQueueMessageReceiverForModel<TModel>(Channel, handler, errorHandler, onIdle, autoComplete);
            await this.ConfigureAsyncHandler(_streamname, receiver, false, cancellationToken, Offset);

        }
        public async Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            var message = model.CreateMessagebusMessage();
            await SendAsync(message, cancellationToken);
        }
        public async Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            await InitializeAsync(cancellationToken);
            await Channel.SendMessageAsync("", _streamname, message);
        }
        public async Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            await InitializeAsync(cancellationToken);
            foreach (var message in messages)
            {
                await Channel.SendMessageAsync("", _streamname, message, cancellationToken: cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
        public async Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
        {
            await SendAsync(models.Select(x => x.CreateMessagebusMessage()), cancellationToken);
        }
        public async Task CloseAsync(CancellationToken cancellationToken = default)
        {
            if (Channel is not null)
                await Channel.CloseAsync(cancellationToken);
            await Connection.CloseAsync(cancellationToken);
        }
        protected override void Dispose(bool disposing)
        {
            CloseAsync().Wait();
        }
    }
}
