
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Mocks;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusStandaloneQueueClientMocked : MessageBusStandaloneQueueClientMock, IMessageBusStandaloneQueueClient, IServiceBusClient
    {
        public ServiceBusStandaloneQueueClientMocked() : base()
        {
        }

        public override Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        public override Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {

        }
        public override void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
        }
        public override Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        public override Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        public override Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        public override Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        public override Task Close()
        {
            return Task.CompletedTask;
        }
    }
}
