using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusQueueAsyncClient(ILogger<ServiceBusQueueAsyncClient> logger, IOptions<MessageBusOptions> messageOptions) 
        : ServiceBusStandaloneQueueAsyncClient(logger, messageOptions.Value.ConnectionString, messageOptions.Value.QueueName)
    {
    }
}
