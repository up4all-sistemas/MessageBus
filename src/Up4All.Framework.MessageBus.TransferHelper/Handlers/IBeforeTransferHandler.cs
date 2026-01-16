using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.TransferHelper.Handlers
{
    public interface IBeforeTransferHandler
    {
        Task<bool> CanTransfer(ReceivedMessage receivedMessage, CancellationToken cancellationToken);

        Task OnBeforeTransfer(MessageBusMessage destinationMessage, CancellationToken cancellationToken);
    }
}
