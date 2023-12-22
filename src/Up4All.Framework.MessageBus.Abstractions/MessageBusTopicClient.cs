using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusTopicClient : MessageBusClientBase, IMessageBusPublisher
    {
        public MessageBusTopicClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions)
        {
        }

        public abstract Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default);

        public abstract Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default);
        public abstract Task SendAsync<TModel>(TModel model, CancellationToken cancellation = default);
        public abstract Task SendManyAsync<TModel>(IEnumerable<TModel> list, CancellationToken cancellation = default);
    }
}
