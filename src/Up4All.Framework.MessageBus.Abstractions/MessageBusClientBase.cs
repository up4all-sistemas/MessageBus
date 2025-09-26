using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusClientBase(IOptions<MessageBusOptions> messageBusOptions) : MessageBusStandaloneClientBase()
    {
        protected readonly MessageBusOptions MessageBusOptions = messageBusOptions.Value;
    }
}
