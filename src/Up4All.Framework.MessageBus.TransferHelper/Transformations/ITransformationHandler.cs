using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.TransferHelper.Transformations
{
    public interface ITransformationHandler
    {
        Task<MessageBusMessage> TransformAsync(ReceivedMessage receivedMessage, CancellationToken cancellationToken);
    }
}
