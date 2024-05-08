
using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusQueueClient : MessageBusQueueClient, IServiceBusClient
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _queueClient;
        private readonly MessageBusOptions _opts;
        private ServiceBusProcessor _processor;

        public ServiceBusQueueClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions)
        {
            _opts = messageOptions.Value;
            (_client, _queueClient) = ServiceBusClientExtensions.CreateClient(messageOptions.Value);
        }

        public override async Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            _processor = CreateProcessor(autoComplete);
            await _processor.RegisterHandleMessageAsync(handler, errorHandler, onIdle, autoComplete, cancellationToken);
            await _processor.StartProcessingAsync(cancellationToken);
        }

        public override async Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            _processor = CreateProcessor(autoComplete);
            await _processor.RegisterHandleMessageAsync(handler, errorHandler, onIdle, autoComplete, cancellationToken);
            await _processor.StartProcessingAsync(cancellationToken);
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _processor = CreateProcessor(autoComplete);
            _processor.RegisterHandleMessage(handler, errorHandler, onIdle, autoComplete);
            _processor.StartProcessingAsync().Wait();
        }

        public override void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _processor = CreateProcessor(autoComplete);
            _processor.RegisterHandleMessage(handler, errorHandler, onIdle, autoComplete);
            _processor.StartProcessingAsync().Wait();
        }

        private ServiceBusProcessor CreateProcessor(bool autoComplete)
        {
            return _client.CreateProcessor(_opts.QueueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = autoComplete,
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(6)
            });
        }

        public override async Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            await _queueClient.SendMessageAsync(ServiceBusClientExtensions.PrepareMesssage(message), cancellationToken);
        }

        public override async Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            var sbMessages = messages.Select(x => ServiceBusClientExtensions.PrepareMesssage(x));
            await _queueClient.SendMessagesAsync(sbMessages, cancellationToken);
        }

        public override async Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            await _queueClient.SendMessageAsync(ServiceBusClientExtensions.PrepareMesssage(model), cancellationToken);
        }

        public override async Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
        {
            var sbMessages = models.Select(x => ServiceBusClientExtensions.PrepareMesssage(x));
            await _queueClient.SendMessagesAsync(sbMessages, cancellationToken);
        }

        public override async Task Close()
        {
            if (_processor != null) await _processor.CloseAsync();
            if (_queueClient != null) await _queueClient.CloseAsync();
            if (_client != null) await _client.DisposeAsync().AsTask();
        }

        protected override void Dispose(bool disposing)
        {
            Close().Wait();
        }
    }
}
