using Microsoft.Extensions.Logging;

using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.TransferHelper.Handlers
{
    public class DefaultBeforeTransferHandler(ILogger<DefaultBeforeTransferHandler> logger) : IBeforeTransferHandler
    {
        protected ILogger<DefaultBeforeTransferHandler> _logger = logger;

        public virtual Task<bool> CanTransfer(ReceivedMessage receivedMessage, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Running default CanTransfer");
            return Task.FromResult(true);
        }

        public virtual Task OnBeforeTransfer(MessageBusMessage destinationMessage, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Running default OnBeforeTransfer");
            return Task.CompletedTask;
        }
    }
}
