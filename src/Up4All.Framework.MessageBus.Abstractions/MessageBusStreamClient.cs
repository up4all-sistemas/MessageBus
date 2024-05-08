﻿using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStreamClient : MessageBusClientBase, IMessageBusStreamClient
    {
        protected readonly object Offset;

        protected MessageBusStreamClient(IOptions<MessageBusOptions> messageBusOptions, object offset) : base(messageBusOptions)
        {
            this.Offset = offset;
        }

        public abstract void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false);
        public abstract void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false);
        public abstract Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default);
        public abstract Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default);
        public abstract Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default);
        public abstract Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default);
        public abstract Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default);
        public abstract Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default);
        public abstract Task Close();
    }
}
