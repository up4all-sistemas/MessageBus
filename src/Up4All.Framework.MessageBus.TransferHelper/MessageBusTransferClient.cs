using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.TransferHelper.Handlers;
using Up4All.Framework.MessageBus.TransferHelper.Options;
using Up4All.Framework.MessageBus.TransferHelper.Transformations;

namespace Up4All.Framework.MessageBus.TransferHelper
{
    public class MessageBusTransferClient<TOptionsSource, TOptionsDest>(IMessageBusAsyncConsumer consumer, IMessageBusPublisherAsync publisher
            , ILogger<MessageBusTransferClient<TOptionsSource, TOptionsDest>> logger
            , IOptions<TransferOptions<TOptionsSource, TOptionsDest>> opts
            , IBeforeTransferHandler beforeTransferHandler
            , ITransformationHandler tranformHandler)
        : IMessageBusTransferClient, IDisposable
        where TOptionsSource : MessageBusOptions, new()
        where TOptionsDest : MessageBusOptions, new()
    {
        private readonly IMessageBusAsyncConsumer _consumer = consumer;
        private readonly IMessageBusPublisherAsync _publisher = publisher;
        private readonly IBeforeTransferHandler _beforeTransferHandler = beforeTransferHandler;
        private readonly ITransformationHandler _transformHandler = tranformHandler;
        private readonly ILogger<MessageBusTransferClient<TOptionsSource, TOptionsDest>> _logger = logger;
        private readonly TransferTransformations? _transformationsOptions = opts.Value.Transformations;

        public Task StartAsync(CancellationToken cancellationToken) => _consumer.RegisterHandlerAsync(OnProcessAsync, OnErrorAsync, OnIdleAsync, false, cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) => _consumer.CloseAsync(cancellationToken);

        protected virtual async Task<MessageReceivedStatus> OnProcessAsync(ReceivedMessage sourceMessage, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Receiving message from {SourceQueueName} for transfer to {DestinationTopicName}", _consumer.QueueName, _publisher.TopicName);

                if (!await _beforeTransferHandler.CanTransfer(sourceMessage, cancellationToken))
                    return MessageReceivedStatus.Completed;

                var destMessage = await _transformHandler.TransformAsync(sourceMessage, _transformationsOptions, cancellationToken);

                await _beforeTransferHandler.OnBeforeTransfer(destMessage, cancellationToken);

                destMessage.AddUserProperties(new Dictionary<string, object>
                {
                    { "mb-transfer-from", _consumer.QueueName },
                    { "mb-transfer-timestamp", DateTime.Now.ToString() }
                });

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
