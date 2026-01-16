using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusTopicAsyncClient(ILogger<ServiceBusTopicAsyncClient> logger, IOptions<MessageBusOptions> messageOptions) 
        : ServiceBusStandaloneTopicAsyncClient(logger, messageOptions.Value.ConnectionString, messageOptions.Value.TopicName)
    {
    }
}
