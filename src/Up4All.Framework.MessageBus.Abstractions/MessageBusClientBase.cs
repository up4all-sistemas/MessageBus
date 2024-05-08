using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusClientBase : MessageBusStandaloneClientBase
    {
        protected readonly MessageBusOptions MessageBusOptions;

        protected MessageBusClientBase(IOptions<MessageBusOptions> messageBusOptions) : base()
        {
            MessageBusOptions = messageBusOptions.Value;
        }
    }
}
