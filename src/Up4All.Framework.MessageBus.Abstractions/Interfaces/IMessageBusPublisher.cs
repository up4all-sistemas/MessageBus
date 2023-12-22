using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Abstractions.Interfaces
{
    public interface IMessageBusPublisher
    {
        Task SendAsync<TModel>(TModel model, CancellationToken cancellation = default);

        Task SendManyAsync<TModel>(IEnumerable<TModel> list, CancellationToken cancellation = default);

        Task SendAsync(MessageBusMessage message, CancellationToken cancellation = default);

        Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellation = default);
    }
}
