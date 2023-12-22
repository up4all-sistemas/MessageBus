
using Azure.Messaging.ServiceBus;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneQueueClient : MessageBusStandaloneQueueClient, IServiceBusClient, IDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _queueClient;
        private readonly string _queueName;
        private ServiceBusProcessor _processor;

        public ServiceBusStandaloneQueueClient(string connectionString, string queuename, int connectionAttemps = 8) : base(connectionString, queuename)
        {
            _queueName = queuename;
            (_client, _queueClient) = this.CreateClient(connectionString, queuename, connectionAttemps);
        }

        public override async Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            _processor = CreateProcessor(autoComplete);
            await _processor.RegisterHandleMessageAsync(handler, errorHandler, onIdle, autoComplete, cancellationToken);
            await _processor.StartProcessingAsync();
        }

        public override async Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            _processor = CreateProcessor(autoComplete);
            await _processor.RegisterHandleMessageAsync(handler, errorHandler, onIdle, autoComplete, cancellationToken);
            await _processor.StartProcessingAsync();
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _processor = CreateProcessor(autoComplete);
            _processor.RegisterHandleMessage(handler, errorHandler, onIdle, autoComplete);
            _processor.StartProcessingAsync().Wait();
        }

        public override void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _processor = CreateProcessor(autoComplete);
            _processor.RegisterHandleMessage(handler, errorHandler, onIdle, autoComplete);
            _processor.StartProcessingAsync().Wait();
        }

        private ServiceBusProcessor CreateProcessor(bool autoComplete)
        {
            return _client.CreateProcessor(_queueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = autoComplete
            });
        }

        public override async Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            await _queueClient.SendMessageAsync(this.PrepareMesssage(message), cancellationToken);
        }

        public override async Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            var sbMessages = messages.Select(x => this.PrepareMesssage(x));
            await _queueClient.SendMessagesAsync(sbMessages, cancellationToken);
        }

        public override async Task SendAsync<TModel>(TModel message, CancellationToken cancellationToken = default)
        {
            await _queueClient.SendMessageAsync(this.PrepareMesssage(message), cancellationToken);
        }

        public override async Task SendManyAsync<TModel>(IEnumerable<TModel> messages, CancellationToken cancellationToken = default)
        {
            var sbMessages = messages.Select(x => this.PrepareMesssage(x));
            await _queueClient.SendMessagesAsync(sbMessages, cancellationToken);
        }

        public void Dispose()
        {
            Close().Wait();
        }

        public override async Task Close()
        {
            await _processor?.CloseAsync();
            await _queueClient?.CloseAsync();
            await _queueClient?.DisposeAsync().AsTask();
        }

    }
}
