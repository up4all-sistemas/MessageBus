using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStreamClient(IOptions<MessageBusOptions> messageBusOptions, object offset) : MessageBusClientBase(messageBusOptions)
    {
        protected readonly object Offset = offset;
    }
}
