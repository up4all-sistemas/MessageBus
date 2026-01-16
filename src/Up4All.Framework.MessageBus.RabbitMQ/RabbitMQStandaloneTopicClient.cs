using Microsoft.Extensions.Logging;

using RabbitMQ.Client;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQStandaloneTopicAsyncClient(ILogger<RabbitMQStandaloneTopicAsyncClient> logger, string connectionString, string topicName, int connectionAttemps = 8, string type = ExchangeType.Topic
            , ExchangeDeclareOptions declareOpts = null)
        : MessageBusStandaloneTopicClient(connectionString, topicName, connectionAttemps), IRabbitMQClient, IMessageBusStandalonePublisherAsync
    {
        private readonly string _topicName = topicName;
        private readonly string _type = type;
        private readonly ExchangeDeclareOptions _declareOpts = declareOpts;
        protected readonly ILogger<RabbitMQStandaloneTopicAsyncClient> _logger = logger;

        public IConnection Connection { get; set; }

        public IChannel Channel { get; private set; }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await this.GetConnectionAsync(ConnectionString, ConnectionAttempts, cancellationToken);

            if (Channel is not null) return;
            Channel = await this.CreateChannelAsync(cancellationToken);

            if (_declareOpts is null) return;
            await Channel.ExchangeDeclareAsync(_topicName, _type, _declareOpts.Durable, _declareOpts.AutoDelete, _declareOpts.Args);
        }

        public async Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            var message = model.CreateMessagebusMessage();
            await SendAsync(message, cancellationToken);
        }

        public async Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            await InitializeAsync(cancellationToken);
            await Channel.SendMessageAsync(_logger, _topicName, string.Empty, message, cancellationToken: cancellationToken);
        }

        public async Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            await InitializeAsync(cancellationToken);
            foreach (var message in messages)
                await SendAsync(message, cancellationToken);
        }

        public async Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
        {
            await InitializeAsync(cancellationToken);
            await SendAsync(models.Select(x => x.CreateMessagebusMessage()), cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            Channel?.CloseAsync().Wait();
            Connection?.CloseAsync().Wait();
        }
    }
}
