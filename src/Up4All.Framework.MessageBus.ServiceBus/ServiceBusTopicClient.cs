using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusTopicClient(IOptions<MessageBusOptions> messageOptions) : ServiceBusStandaloneTopicClient(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName)
    {
    }

    public class ServiceBusTopicAsyncClient(IOptions<MessageBusOptions> messageOptions) : ServiceBusStandaloneTopicAsyncClient(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName)
    {
    }
}
