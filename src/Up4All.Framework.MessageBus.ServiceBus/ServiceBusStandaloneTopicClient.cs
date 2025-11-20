
using Azure.Messaging.ServiceBus;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneTopicClient : MessageBusStandaloneTopicClient, IMessageBusStandalonePublisher, IServiceBusClient
    {
        private readonly ServiceBusSender _topicClient;

        public ServiceBusStandaloneTopicClient(string connectionString, string topicName, int connectionAttempts = 8) : base(connectionString, topicName)
        {
            var (_, topicClient) = ServiceBusClientExtensions.CreateClient(connectionString, topicName, connectionAttempts);
            _topicClient = topicClient;
        }

        public void Send(MessageBusMessage message)
        {
            _topicClient.SendMessageAsync(ServiceBusClientExtensions.PrepareMesssage(message)).Wait();
        }

        public void Send(IEnumerable<MessageBusMessage> messages)
        {
            var sbMessages = messages.Select(ServiceBusClientExtensions.PrepareMesssage);
            _topicClient.SendMessagesAsync(sbMessages).Wait();
        }

        public void Send<TModel>(TModel model)
        {
            _topicClient.SendMessageAsync(ServiceBusClientExtensions.PrepareMesssage(model)).Wait();
        }

        public void SendMany<TModel>(IEnumerable<TModel> models)
        {
            var sbMessages = models.Select(ServiceBusClientExtensions.PrepareMesssage);
            _topicClient.SendMessagesAsync(sbMessages).Wait();
        }

        protected override void Dispose(bool disposing)
        {
        }
    }

    public class ServiceBusStandaloneTopicAsyncClient : MessageBusStandaloneTopicClient, IMessageBusStandalonePublisherAsync, IServiceBusClient
    {
        private readonly ServiceBusSender _topicClient;

        public ServiceBusStandaloneTopicAsyncClient(string connectionString, string topicName, int connectionAttempts = 8) : base(connectionString, topicName)
        {
            var (_, topicClient) = ServiceBusClientExtensions.CreateClient(connectionString, topicName, connectionAttempts);
            _topicClient = topicClient;
        }

        public async Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            await _topicClient.SendMessageAsync(ServiceBusClientExtensions.PrepareMesssage(message), cancellationToken);
        }

        public async Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            var sbMessages = messages.Select(ServiceBusClientExtensions.PrepareMesssage);
            await _topicClient.SendMessagesAsync(sbMessages, cancellationToken);
        }

        public async Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            await _topicClient.SendMessageAsync(ServiceBusClientExtensions.PrepareMesssage(model), cancellationToken);
        }

        public async Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
        {
            var sbMessages = models.Select(ServiceBusClientExtensions.PrepareMesssage);
            await _topicClient.SendMessagesAsync(sbMessages, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}
