using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Abstractions.Consumers
{
    public class DefaultConsumer<THandler>
        : IHostedService, IDisposable, IAsyncDisposable
        where THandler : notnull, IMessageBusMessageHandler
    {
        private readonly IMessageBusAsyncConsumer _consumer;
        private readonly IMessageBusMessageHandler _handler;
        private readonly ILogger<DefaultConsumer<THandler>> _logger;

        public DefaultConsumer(IMessageBusAsyncConsumer consumer, IMessageBusMessageHandler handler, ILogger<DefaultConsumer<THandler>> logger)
        {
            _consumer = consumer;
            _handler = handler;
            _logger = logger;
        }

        public DefaultConsumer(object serviceKey, IServiceProvider serviceProvider)
        {
            _consumer = serviceProvider.GetRequiredKeyedService<IMessageBusAsyncConsumer>(serviceKey);
            _handler = serviceProvider.GetRequiredKeyedService<IMessageBusMessageHandler>(serviceKey);
            _logger = serviceProvider.GetRequiredService<ILogger<DefaultConsumer<THandler>>>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            // The generic host already calls StopAsync() (which closes the consumer)
            // during a graceful shutdown; CloseAsync() below is a no-op in that case and
            // only actually closes the connection when DisposeAsync() is called directly
            // without StopAsync() having run first.
            await _consumer.CloseAsync(CancellationToken.None);
            GC.SuppressFinalize(this);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting consumer for {HandlerName} handler", nameof(THandler));
            return _consumer.RegisterHandlerAsync(OnMessageAsync, _handler.OnErrorAsync, autoComplete: false, cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("stopping consumer for {HandlerName} handler", nameof(THandler));
            return _consumer.CloseAsync(cancellationToken);
        }

        private async Task<MessageReceivedStatus> OnMessageAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Received message in {Entitypath}", _consumer.EntityPath);
                _logger.LogDebug("Calling {HandlerName}", nameof(THandler));
                await _handler.OnMessageReceivedAsync(_consumer.EntityPath, message, cancellationToken);
                return MessageReceivedStatus.Completed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while running {HandlerName}", nameof(THandler));
                await _handler.OnErrorAsync(ex, cancellationToken);
                return MessageReceivedStatus.Abandoned;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _consumer.CloseAsync(CancellationToken.None).Wait();
        }

        ~DefaultConsumer()
        {
            Dispose(false);
        }
    }
}
