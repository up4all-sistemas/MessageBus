using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusSubscribeClient(IOptions<MessageBusOptions> messageBusOptions) : MessageBusClientBase(messageBusOptions)
    {
    }
}
