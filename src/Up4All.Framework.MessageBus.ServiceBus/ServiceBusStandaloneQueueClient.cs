
using Azure.Messaging.ServiceBus;

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
    public class ServiceBusStandaloneQueueClient : MessageBusStandaloneQueueClient, IMessageBusStandaloneQueueClient, IServiceBusClient
    {
        private readonly ServiceBusSender _queueClient;
        private readonly string _queueName;
        private readonly ServiceBusClient _client;
        private ServiceBusProcessor _processor;

        public ServiceBusStandaloneQueueClient(string connectionString, string queuename, int connectionAttemps = 8) : base(connectionString, queuename)
        {
            _queueName = queuename;
            (_client, _queueClient) = ServiceBusClientExtensions.CreateClient(connectionString, queuename, connectionAttemps);
        }


        public void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _processor = _client.CreateQueueProcessor(_queueName, autoComplete);
            _processor.RegisterHandleMessage(handler, errorHandler, onIdle, autoComplete);
            _processor.StartProcessingAsync().Wait();
        }

        public void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
            => RegisterHandler((msg) => handler(msg.GetBody<TModel>()), errorHandler, onIdle, autoComplete);
                
        public void Send(MessageBusMessage message)
        {
            _queueClient.SendMessageAsync(ServiceBusClientExtensions.PrepareMesssage(message)).Wait();
        }

        public void Send(IEnumerable<MessageBusMessage> messages)
        {
            var sbMessages = messages.Select(ServiceBusClientExtensions.PrepareMesssage);
            _queueClient.SendMessagesAsync(sbMessages).Wait();
        }

        public void Send<TModel>(TModel model)
        {
            _queueClient.SendMessageAsync(ServiceBusClientExtensions.PrepareMesssage(model)).Wait();
        }

        public void SendMany<TModel>(IEnumerable<TModel> models)
        {
            var sbMessages = models.Select(x => ServiceBusClientExtensions.PrepareMesssage(x));
            _queueClient.SendMessagesAsync(sbMessages).Wait();
        }

        public void Close()
        {
            _processor?.CloseAsync().Wait();
            _queueClient?.CloseAsync().Wait();
            _queueClient?.DisposeAsync();
        }

        protected override void Dispose(bool disposing)
        {
            Close();
        }
    }

    public class ServiceBusStandaloneQueueAsyncClient : MessageBusStandaloneQueueClient, IMessageBusStandaloneQueueAsyncClient, IServiceBusClient
    {
        private readonly ServiceBusSender _queueClient;
        private readonly string _queueName;
        private readonly ServiceBusClient _client;
        private ServiceBusProcessor _processor;

        public ServiceBusStandaloneQueueAsyncClient(string connectionString, string queuename, int connectionAttemps = 8) : base(connectionString, queuename)
        {
            _queueName = queuename;
            (_client, _queueClient) = ServiceBusClientExtensions.CreateClient(connectionString, queuename, connectionAttemps);
        }

        public async Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            _processor = _client.CreateQueueProcessor(_queueName, autoComplete);
            await _processor.RegisterHandleMessageAsync(handler, errorHandler, onIdle, autoComplete, cancellationToken);
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

        public async Task Close()
        {
            if (_processor != null) await _processor.CloseAsync();
            if (_queueClient != null) await _queueClient.CloseAsync();
            if (_queueClient != null) await _queueClient.DisposeAsync().AsTask();
        }

        protected override void Dispose(bool disposing)
        {
            Close().Wait();
        }
    }
}
