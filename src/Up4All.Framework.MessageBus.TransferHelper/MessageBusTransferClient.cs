using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.TransferHelper.Transformations;

namespace Up4All.Framework.MessageBus.TransferHelper
{
    public class MessageBusTransferClient(IMessageBusAsyncConsumer consumer, IMessageBusPublisherAsync publisher
            , ILogger<MessageBusTransferClient> logger
            , ITransformationHandler? tranformHandler = null)
        : IMessageBusTransferClient, IDisposable
    {
        private readonly IMessageBusAsyncConsumer _consumer = consumer;
        private readonly IMessageBusPublisherAsync _publisher = publisher;
        private readonly ITransformationHandler? _transformHandler = tranformHandler;
        private readonly ILogger<MessageBusTransferClient> _logger = logger;

        public Task StartAsync(CancellationToken cancellationToken) => _consumer.RegisterHandlerAsync(OnProcessAsync, OnErrorAsync, OnIdleAsync, false, cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) => _consumer.CloseAsync(cancellationToken);

        protected virtual async Task<MessageReceivedStatus> OnProcessAsync(ReceivedMessage sourceMessage, CancellationToken cancellationToken)
        {
            try
            {
                MessageBusMessage destMessage = sourceMessage;

                if (_transformHandler is not null)
                    destMessage = await _transformHandler.TransformAsync(sourceMessage, cancellationToken);

                await _publisher.SendAsync(destMessage, cancellationToken);
                return MessageReceivedStatus.Completed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                return MessageReceivedStatus.Abandoned;
            }
        }

        protected virtual Task OnErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "{Message}", exception.Message);
            return Task.CompletedTask;
        }

        protected virtual Task OnIdleAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Waiting for a message to transfer");
            return Task.CompletedTask;
        }

        protected virtual async Task Dispose(bool disposing)
        {
            await _consumer.CloseAsync(CancellationToken.None);            
        }

        public async void Dispose()
        {
            await Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
