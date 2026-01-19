
using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.ServiceBus.Extensions;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneSubscriptionAsyncClient(ILogger<ServiceBusStandaloneSubscriptionAsyncClient> logger, string connectionString, string topicName, string subscriptionName, int connectionAttempts = 8) : MessageBusStandaloneSubscriptonClient(connectionString, topicName, subscriptionName), IMessageBusStandaloneAsyncConsumer, IServiceBusClient
    {
        private readonly ServiceBusClient _client = ServiceBusClientExtensions.CreateClient(connectionString, connectionAttempts);
        private readonly string _topicName = topicName;
        private readonly string _subscriptionName = subscriptionName;
        protected readonly ILogger<ServiceBusStandaloneSubscriptionAsyncClient> _logger = logger;
        private ServiceBusProcessor _processor;

        public async Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            _processor = _client.CreateTopicProcessor(_topicName, _subscriptionName, autoComplete);
            await _processor.RegisterHandleMessageAsync(_logger, handler, errorHandler, onIdle, autoComplete);

            _logger.LogDebug("Start listening {EntityPath}", _processor.EntityPath);
            await _processor.StartProcessingAsync();
        }

        public Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
            => RegisterHandlerAsync((msg, ct) => handler(msg.GetBody<TModel>(), ct), errorHandler, onIdle, autoComplete, cancellationToken);

        public async Task CloseAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Closing connection to {EntityPath}", _processor.EntityPath);
            if (_processor != null) await _processor.CloseAsync(cancellationToken);
            if (_client != null) await _client.DisposeAsync().AsTask();
        }

        protected override void Dispose(bool disposing)
        {
            CloseAsync(CancellationToken.None).Wait();
        }
    }
}
