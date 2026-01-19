
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.TransferHelper.Handlers;
using Up4All.Framework.MessageBus.TransferHelper.Options;
using Up4All.Framework.MessageBus.TransferHelper.Transformations;

namespace Up4All.Framework.MessageBus.TransferHelper
{
    public class ConsumerHandler<TOptionsSource, TOptionsDest>(IMessageBusPublisherAsync publisher
            , ILogger<ConsumerHandler<TOptionsSource, TOptionsDest>> logger
            , IOptions<TransferOptions<TOptionsSource, TOptionsDest>> opts
            , IBeforeTransferHandler beforeTransferHandler
            , ITransformationHandler tranformHandler)
        : IMessageBusMessageHandler
        where TOptionsSource : MessageBusOptions
        where TOptionsDest : MessageBusOptions
    {
        private readonly IMessageBusPublisherAsync _publisher = publisher;
        private readonly IBeforeTransferHandler _beforeTransferHandler = beforeTransferHandler;
        private readonly ITransformationHandler _transformHandler = tranformHandler;
        private readonly ILogger<ConsumerHandler<TOptionsSource, TOptionsDest>> _logger = logger;
        private readonly TransferTransformations? _transformationsOptions = opts.Value.Transformations;

        public async Task OnMessageReceivedAsync(string entityPath, ReceivedMessage sourceMessage, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Receiving message from {SourceQueueName} for transfer to {DestinationTopicName}", entityPath, _publisher.TopicName);

            if (!await _beforeTransferHandler.CanTransfer(sourceMessage, cancellationToken))
            {
                _logger.LogDebug("Skipping message {MessageId} transfer", sourceMessage.GetMessageId<object>());
                return;
            }

            var destMessage = await _transformHandler.TransformAsync(sourceMessage, _transformationsOptions, cancellationToken);

            await _beforeTransferHandler.OnBeforeTransfer(destMessage, cancellationToken);

            destMessage.AddUserProperties(new Dictionary<string, object>
                {
                    { "mb-transfer-from", entityPath },
                    { "mb-transfer-timestamp", DateTime.Now.ToString() }
                });

            await _publisher.SendAsync(destMessage, cancellationToken);
        }

        public Task OnErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "{Message}", exception.Message);
            return Task.CompletedTask;
        }
    }
}
