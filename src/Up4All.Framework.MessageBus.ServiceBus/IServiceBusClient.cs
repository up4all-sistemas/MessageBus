using Azure.Messaging.ServiceBus;

namespace Up4All.Framework.MessageBus.ServiceBus
{
    public interface IServiceBusClient
    {
        /// <summary>
        /// The underlying Azure SDK client this instance sends/receives through. Exposed so
        /// callers (e.g. the health check) can inspect its connection state without the
        /// library needing to guess which registered client "the" connection belongs to.
        /// </summary>
        ServiceBusClient Client { get; }
    }
}
