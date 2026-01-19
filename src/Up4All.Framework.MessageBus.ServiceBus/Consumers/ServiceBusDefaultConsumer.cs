using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.ServiceBus.Consumers
{
    public class ServiceBusDefaultConsumer(IMessageBusAsyncConsumer consumer, IMessageBusMessageHandler handler)
        : IMessageDefaultConsumer, IDisposable
    {
        private readonly IMessageBusAsyncConsumer _consumer = consumer;
        private readonly IMessageBusMessageHandler _handler = handler;

        public void Dispose()
        {
            Dispose(true);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _consumer.RegisterHandlerAsync(OnMessageAsync, _handler.OnErrorAsync, autoComplete: false, cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _consumer.CloseAsync(cancellationToken);
        }

        private async Task<MessageReceivedStatus> OnMessageAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            try
            {
                await _handler.OnMessageReceivedAsync(_consumer.EntityPath, message, cancellationToken);
                return MessageReceivedStatus.Completed;
            }
            catch (Exception ex)
            {
                await _handler.OnErrorAsync(ex, cancellationToken);
                return MessageReceivedStatus.Abandoned;
            }
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
                _consumer.CloseAsync(CancellationToken.None).Wait();
        }

        ~ServiceBusDefaultConsumer()
        {
            Dispose(false);
        }
    }
}
