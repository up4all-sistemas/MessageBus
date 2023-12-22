using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusClientBase
    {
        protected readonly MessageBusOptions MessageBusOptions;

        public MessageBusClientBase(IOptions<MessageBusOptions> messageBusOptions)
        {
            MessageBusOptions = messageBusOptions.Value;
        }
    }
}
