
using Azure.Messaging.ServiceBus;

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
    public class ServiceBusStandaloneSubscribeClient(string connectionString, string topicName, string subscriptionName, int connectionAttempts = 8) 
        : MessageBusStandaloneSubscribeClient(connectionString, topicName, subscriptionName), IMessageBusStandaloneConsumer, IServiceBusClient
    {
        private readonly ServiceBusClient _client = ServiceBusClientExtensions.CreateClient(connectionString, connectionAttempts);
        private readonly string _topicName = topicName;
        private readonly string _subscriptionName = subscriptionName;
        private ServiceBusProcessor _processor;

        public void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _processor = _client.CreateTopicProcessor(_topicName, _subscriptionName, autoComplete);
            _processor.RegisterHandleMessage(handler, errorHandler, onIdle, autoComplete);
            _processor.StartProcessingAsync().Wait();
        }

        public void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
            => RegisterHandler((msg) => handler(msg.GetBody<TModel>()), errorHandler, onIdle, autoComplete);

        public void Close()
        {
            _processor?.CloseAsync().Wait();
            _client?.DisposeAsync();
        }

        protected override void Dispose(bool disposing)
        {
            Close();
        }
    }

    public class ServiceBusStandaloneSubscribeAsyncClient(string connectionString, string topicName, string subscriptionName, int connectionAttempts = 8) : MessageBusStandaloneSubscribeClient(connectionString, topicName, subscriptionName), IMessageBusStandaloneAsyncConsumer, IServiceBusClient
    {
        private readonly ServiceBusClient _client = ServiceBusClientExtensions.CreateClient(connectionString, connectionAttempts);
        private readonly string _topicName = topicName;
        private readonly string _subscriptionName = subscriptionName;
        private ServiceBusProcessor _processor;

        public async Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            _processor = _client.CreateTopicProcessor(_topicName, _subscriptionName, autoComplete);
            await _processor.RegisterHandleMessageAsync(handler, errorHandler, onIdle, autoComplete);
            await _processor.StartProcessingAsync();
        }

        public Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
            => RegisterHandlerAsync((msg, ct) => handler(msg.GetBody<TModel>(), ct), errorHandler, onIdle, autoComplete, cancellationToken);
        
        public async Task Close()
        {
            if (_processor != null) await _processor.CloseAsync();
            if (_client != null) await _client.DisposeAsync().AsTask();
        }

        protected override void Dispose(bool disposing)
        {
            Close().Wait();
        }
    }
}
