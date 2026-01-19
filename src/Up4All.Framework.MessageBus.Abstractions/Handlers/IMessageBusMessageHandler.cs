using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Abstractions.Handlers
{
    public interface IMessageBusMessageHandler
    {
        Task OnMessageReceivedAsync(string entityPath, ReceivedMessage message, CancellationToken cancellationToken);

        Task OnErrorAsync(Exception exception, CancellationToken cancellationToken);

    }

    public interface IMessageBusMessageHandler<TMessage> : IMessageBusMessageHandler
        where TMessage : class
    {
        Task OnMessageReceivedAsync(TMessage message, CancellationToken cancellationToken);
    }
}
