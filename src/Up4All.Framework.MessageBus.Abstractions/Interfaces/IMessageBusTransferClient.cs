using System.Threading;
using System.Threading.Tasks;

namespace Up4All.Framework.MessageBus.Abstractions.Interfaces
{
    public interface IMessageBusTransferClient
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
