using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusTopicClient : ServiceBusStandaloneTopicClient
    {
        public ServiceBusTopicClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName)
        {
        }
    }

    public class ServiceBusTopicAsyncClient : ServiceBusStandaloneTopicAsyncClient
    {
        public ServiceBusTopicAsyncClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName)
        {
        }
    }
}
