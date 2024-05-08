using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStreamClient : MessageBusClientBase
    {
        protected readonly object Offset;

        protected MessageBusStreamClient(IOptions<MessageBusOptions> messageBusOptions, object offset) : base(messageBusOptions)
        {
            this.Offset = offset;
        }
    }
}
