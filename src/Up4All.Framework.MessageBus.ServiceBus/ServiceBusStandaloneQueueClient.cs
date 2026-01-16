
using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.ServiceBus.Extensions;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneQueueAsyncClient : MessageBusStandaloneQueueClient, IMessageBusStandaloneQueueAsyncClient, IServiceBusClient
    {
        private readonly ServiceBusSender _queueClient;
        private readonly string _queueName;
        private readonly ServiceBusClient _client;
        protected readonly ILogger<ServiceBusStandaloneQueueAsyncClient> _logger;
        private ServiceBusProcessor _processor;

        public ServiceBusStandaloneQueueAsyncClient(ILogger<ServiceBusStandaloneQueueAsyncClient> logger, string connectionString, string queuename, int connectionAttemps = 8) 
            : base(connectionString, queuename)
        {
            _logger = logger;
            _queueName = queuename;
            (_client, _queueClient) = ServiceBusClientExtensions.CreateClient(_logger, connectionString, queuename, connectionAttemps);
        }

        public async Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            _processor = _client.CreateQueueProcessor(_queueName, autoComplete);
            await _processor.RegisterHandleMessageAsync(_logger, handler, errorHandler, onIdle, autoComplete, cancellationToken);

            _logger.LogDebug("Start listening {EntityPath}", _processor.EntityPath);
            await _processor.StartProcessingAsync();
        }

        public Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
            => RegisterHandlerAsync((msg, ct) => handler(msg.GetBody<TModel>(), ct), errorHandler, onIdle, autoComplete);

        public async Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            await _queueClient.SendMessageAsync(ServiceBusClientExtensions.PrepareMesssage(message), cancellationToken);
        }

        public async Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            var sbMessages = messages.Select(ServiceBusClientExtensions.PrepareMesssage);
            await _queueClient.SendMessagesAsync(sbMessages, cancellationToken);
        }

        public async Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            await _queueClient.SendMessageAsync(ServiceBusClientExtensions.PrepareMesssage(model), cancellationToken);
        }

        public async Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
        {
            var sbMessages = models.Select(ServiceBusClientExtensions.PrepareMesssage);
            await _queueClient.SendMessagesAsync(sbMessages, cancellationToken);
        }

        public async Task CloseAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Closing connection to {EntityPath}", _processor.EntityPath);
            if (_processor != null) await _processor.CloseAsync(cancellationToken);
            if (_queueClient != null) await _queueClient.CloseAsync(cancellationToken);
            if (_queueClient != null) await _queueClient.DisposeAsync().AsTask();
        }

        protected override void Dispose(bool disposing)
        {
            CloseAsync(CancellationToken.None).Wait();
        }
    }
}
