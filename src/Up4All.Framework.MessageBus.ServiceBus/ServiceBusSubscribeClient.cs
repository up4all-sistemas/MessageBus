using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusSubscribeAsyncClient(ILogger<ServiceBusSubscribeAsyncClient> logger, IOptions<MessageBusOptions> messageOptions) 
        : ServiceBusStandaloneSubscribeAsyncClient(logger, messageOptions.Value.ConnectionString, messageOptions.Value.TopicName, messageOptions.Value.SubscriptionName, messageOptions.Value.ConnectionAttempts)
    {
    }
}
