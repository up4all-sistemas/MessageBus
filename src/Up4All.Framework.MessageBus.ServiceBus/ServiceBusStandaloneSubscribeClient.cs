
using Azure.Messaging.ServiceBus;

using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneSubscribeClient(string connectionString, string topicName, string subscriptionName, int connectionAttempts = 8) : MessageBusStandaloneSubscribeClient(connectionString, topicName, subscriptionName), IMessageBusStandaloneConsumer, IServiceBusClient
    {
        private readonly ServiceBusClient _client = ServiceBusClientExtensions.CreateClient(connectionString, connectionAttempts);
        private readonly string _topicName = topicName;
        private readonly string _subscriptionName = subscriptionName;
        private ServiceBusProcessor _processor;

        public void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _processor = CreateProcessor(autoComplete);
            _processor.RegisterHandleMessage(handler, errorHandler, onIdle, autoComplete);
            _processor.StartProcessingAsync().Wait();
        }

        public void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _processor = CreateProcessor(autoComplete);
            _processor.RegisterHandleMessage(handler, errorHandler, onIdle, autoComplete);
            _processor.StartProcessingAsync().Wait();
        }

        private ServiceBusProcessor CreateProcessor(bool autoComplete)
        {
            return _client.CreateProcessor(_topicName, _subscriptionName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = autoComplete,
                MaxConcurrentCalls = 1,
                MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(1),
                PrefetchCount = 0,
            });
        }

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
            _processor = CreateProcessor(autoComplete);
            await _processor.RegisterHandleMessageAsync(handler, errorHandler, onIdle, autoComplete);
            await _processor.StartProcessingAsync();
        }

        public async Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            _processor = CreateProcessor(autoComplete);
            await _processor.RegisterHandleMessageAsync(handler, errorHandler, onIdle, autoComplete);
            await _processor.StartProcessingAsync();
        }

        private ServiceBusProcessor CreateProcessor(bool autoComplete)
        {
            return _client.CreateProcessor(_topicName, _subscriptionName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = autoComplete,
                MaxConcurrentCalls = 1,
                MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(1),
                PrefetchCount = 0,
            });
        }

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
