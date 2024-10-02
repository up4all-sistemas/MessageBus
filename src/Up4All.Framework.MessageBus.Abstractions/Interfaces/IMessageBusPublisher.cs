using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Abstractions.Interfaces
{
    public interface IMessageBusPublisher
    {
        void Send<TModel>(TModel model);

        void Send(MessageBusMessage message);

        void Send(IEnumerable<MessageBusMessage> messages);

        void SendMany<TModel>(IEnumerable<TModel> models);
    }

    public interface IMessageBusPublisherAsync
    {
        Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default);

        Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default);

        Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default);

        Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default);
    }
}
