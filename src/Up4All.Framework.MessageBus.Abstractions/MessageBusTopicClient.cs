using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusTopicClient : MessageBusClientBase
    {
        protected MessageBusTopicClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions)
        {
        }
    }
}
