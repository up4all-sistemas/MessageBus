
using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.ServiceBus.Extensions;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneTopicAsyncClient : MessageBusStandaloneTopicClient, IMessageBusStandalonePublisherAsync, IServiceBusClient
    {
        private readonly ServiceBusSender _topicClient;
        protected readonly ILogger<ServiceBusStandaloneTopicAsyncClient> _logger;

        public ServiceBusStandaloneTopicAsyncClient(ILogger<ServiceBusStandaloneTopicAsyncClient> logger, string connectionString, string topicName, int connectionAttempts = 8) : base(connectionString, topicName)
        {
            _logger = logger;
            var (_, topicClient) = ServiceBusClientExtensions.CreateClient(_logger, connectionString, topicName, connectionAttempts);
            _topicClient = topicClient;
        }

        public Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
            => _topicClient.SendMessageBusMessageAsync(_logger, message, cancellationToken);

        public Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
            => _topicClient.SendMessageBusMessageAsync(_logger, messages, cancellationToken);

        public Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
            => SendAsync(model.CreateMessagebusMessage(), cancellationToken);

        public Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
            => SendAsync(models.Select(x => x.CreateMessagebusMessage()), cancellationToken);

        protected override void Dispose(bool disposing)
        {
            _topicClient.CloseAsync().Wait();
        }
    }
}
