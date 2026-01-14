using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusTopicAsyncClient(IOptions<MessageBusOptions> messageOptions) : ServiceBusStandaloneTopicAsyncClient(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName)
    {
    }
}
