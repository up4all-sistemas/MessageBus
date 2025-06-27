using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Mocks
{
    public abstract class MessageBusStandaloneStreamClientMock : MessageBusClientBaseMock, IMessageBusStandaloneStreamClient
    {
        protected MessageBusStandaloneStreamClientMock() : base()
        {
        }

        public string TopicName => string.Empty;

        public string QueueName => string.Empty;

        public abstract void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false);
        public abstract void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false);
        public abstract void Send(MessageBusMessage message);
        public abstract void Send(IEnumerable<MessageBusMessage> messages);
        public abstract void Send<TModel>(TModel model);
        public abstract void SendMany<TModel>(IEnumerable<TModel> models);
        public abstract void Close();
    }

    public abstract class MessageBusStandaloneStreamAsyncClientMock : MessageBusClientBaseMock, IMessageBusStandaloneStreamAsyncClient
    {
        protected MessageBusStandaloneStreamAsyncClientMock() : base()
        {
        }

        public string TopicName => string.Empty;

        public string QueueName => string.Empty;

        public abstract Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default);
        public abstract Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default);
        public abstract Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default);
        public abstract Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default);
        public abstract Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default);
        public abstract Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default);
        public abstract Task Close();
    }
}
