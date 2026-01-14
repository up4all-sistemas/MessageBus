using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Abstractions.Interfaces
{
    public interface IMessageBusPublisherAsync
    {
        string TopicName { get; }

        Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default);

        Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default);

        Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default);

        Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default);
    }
}
