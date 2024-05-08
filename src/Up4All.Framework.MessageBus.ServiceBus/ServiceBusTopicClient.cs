using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusTopicClient : ServiceBusStandaloneTopicClient, IServiceBusClient
    {
        public ServiceBusTopicClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName)
        {
        }
    }
}
