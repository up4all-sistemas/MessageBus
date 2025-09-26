using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusSubscribeClient(IOptions<MessageBusOptions> messageOptions) : ServiceBusStandaloneSubscribeClient(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName, messageOptions.Value.SubscriptionName, messageOptions.Value.ConnectionAttempts)
    {
    }

    public class ServiceBusSubscribeAsyncClient(IOptions<MessageBusOptions> messageOptions) : ServiceBusStandaloneSubscribeAsyncClient(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName, messageOptions.Value.SubscriptionName, messageOptions.Value.ConnectionAttempts)
    {
    }
}
