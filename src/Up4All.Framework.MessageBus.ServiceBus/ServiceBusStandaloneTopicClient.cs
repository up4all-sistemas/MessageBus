
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
    public class ServiceBusStandaloneTopicClient : MessageBusStandaloneTopicClient, IServiceBusClient
    {
        private readonly ServiceBusSender _topicClient;

        public ServiceBusStandaloneTopicClient(string connectionString, string topicName, int connectionAttempts = 8) : base(connectionString, topicName)
        {
            var (_, topicClient) = ServiceBusClientExtensions.CreateClient(connectionString, topicName, connectionAttempts);
            _topicClient = topicClient;
        }

        public override async Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            await _topicClient.SendMessageAsync(ServiceBusClientExtensions.PrepareMesssage(message), cancellationToken);
        }

        public override async Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            var sbMessages = messages.Select(x => ServiceBusClientExtensions.PrepareMesssage(x));
            await _topicClient.SendMessagesAsync(sbMessages, cancellationToken);
        }

        public override async Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            await _topicClient.SendMessageAsync(ServiceBusClientExtensions.PrepareMesssage(model), cancellationToken);
        }

        public override async Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
        {
            var sbMessages = models.Select(x => ServiceBusClientExtensions.PrepareMesssage(x));
            await _topicClient.SendMessagesAsync(sbMessages, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {            
        }
    }
}
