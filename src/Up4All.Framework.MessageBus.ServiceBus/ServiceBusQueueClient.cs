using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusQueueClient(IOptions<MessageBusOptions> messageOptions) : ServiceBusStandaloneQueueClient(messageOptions.Value.ConnectionString, messageOptions.Value.QueueName)
    {
    }

    public class ServiceBusQueueAsyncClient(IOptions<MessageBusOptions> messageOptions) : ServiceBusStandaloneQueueAsyncClient(messageOptions.Value.ConnectionString, messageOptions.Value.QueueName)
    {
    }
}
