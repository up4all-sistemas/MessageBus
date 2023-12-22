
using Azure.Messaging.ServiceBus;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneTopicClient : MessageBusStandaloneTopicClient, IServiceBusClient, IDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _topicClient;

        public ServiceBusStandaloneTopicClient(string connectionString, string topicName, int connectionAttempts = 8) : base(connectionString, topicName)
        {
            (_client, _topicClient) = this.CreateClient(connectionString, topicName, connectionAttempts);
        }

        public void Dispose()
        {
            _topicClient.CloseAsync().Wait();
        }

        public override async Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            await _topicClient.SendMessageAsync(this.PrepareMesssage(message), cancellationToken);
        }

        public override async Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            var sbMessages = messages.Select(x => this.PrepareMesssage(x));
            await _topicClient.SendMessagesAsync(sbMessages, cancellationToken);
        }

        public override async Task SendAsync<TModel>(TModel message, CancellationToken cancellationToken = default)
        {
            await _topicClient.SendMessageAsync(this.PrepareMesssage(message), cancellationToken);
        }

        public override async Task SendManyAsync<TModel>(IEnumerable<TModel> messages, CancellationToken cancellationToken = default)
        {
            var sbMessages = messages.Select(x => this.PrepareMesssage(x));
            await _topicClient.SendMessagesAsync(sbMessages, cancellationToken);
        }

    }
}
