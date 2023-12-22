
using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Mocks;

namespace Up4All.Framework.MessageBus.RabbitMQ.Mocks
{
    public class RabbitMQQueueClientMocked : MessageBusQueueClientMock, IMessageBusQueueClient, IRabbitMQClient, IDisposable
    {
        public IConnection Connection { get; set; }

        public RabbitMQQueueClientMocked() : base()
        {
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {

        }

        public void Dispose()
        {

        }

        public override Task Close()
        {
            return Task.CompletedTask;
        }

        public override Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override Task SendAsync<TModel>(TModel model, CancellationToken cancellation = default)
        {
            return Task.CompletedTask;
        }

        public override Task SendManyAsync<TModel>(IEnumerable<TModel> list, CancellationToken cancellation = default)
        {
            return Task.CompletedTask;
        }

        public override Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
        }
    }
}
