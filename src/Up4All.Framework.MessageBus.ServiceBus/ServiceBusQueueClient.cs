using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public class ServiceBusQueueClient : ServiceBusStandaloneQueueClient
    {
        public ServiceBusQueueClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions.Value.ConnectionString, messageOptions.Value.QueueName)
        {
        }
    }

    public class ServiceBusQueueAsyncClient : ServiceBusStandaloneQueueAsyncClient
    {
        public ServiceBusQueueAsyncClient(IOptions<MessageBusOptions> messageOptions) : base(messageOptions.Value.ConnectionString, messageOptions.Value.QueueName)
        {
        }
    }
}
