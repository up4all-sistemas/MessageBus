
using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Mocks;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusSubscribeClientMocked : MessageBusSubscribeClientMock, IMessageBusConsumer, IServiceBusClient
    {
        public ServiceBusSubscribeClientMocked() : base()
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
        public override void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
        }
        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
        }

        public override Task Close()
        {
            return Task.CompletedTask;
        }
    }
}
