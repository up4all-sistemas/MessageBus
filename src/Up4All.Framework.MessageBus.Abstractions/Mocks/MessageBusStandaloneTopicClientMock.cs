
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Abstractions.Mocks
{
    public abstract class MessageBusStandaloneTopicClientMock : MessageBusClientBaseMock, IMessageBusStandalonePublisher
    {
        public MessageBusStandaloneTopicClientMock() : base()
        {
        }

        public abstract Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default);

        public abstract Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default);
        public abstract Task SendAsync<TModel>(TModel model, CancellationToken cancellation = default);
        public abstract Task SendManyAsync<TModel>(IEnumerable<TModel> list, CancellationToken cancellation = default);
    }
}
