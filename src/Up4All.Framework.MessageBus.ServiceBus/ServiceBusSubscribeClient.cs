using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusSubscribeClient : ServiceBusStandaloneSubscribeClient, IServiceBusClient
    {
        public ServiceBusSubscribeClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName, messageOptions.Value.SubscriptionName, messageOptions.Value.ConnectionAttempts)
        {
        }
    }
}
