using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Abstractions.Mocks
{
    public abstract class MessageBusStandaloneQueueClientMock : MessageBusClientBaseMock, IMessageBusStandaloneQueueClient
    {
        public MessageBusStandaloneQueueClientMock() : base()
        {
        }

        public abstract Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default);

        public abstract void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false);

        public abstract Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default);

        public abstract Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default);

        public abstract Task Close();
        public abstract Task SendAsync<TModel>(TModel model, CancellationToken cancellation = default);
        public abstract Task SendManyAsync<TModel>(IEnumerable<TModel> list, CancellationToken cancellation = default);
        public abstract Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default);
        public abstract void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false);
    }
}
