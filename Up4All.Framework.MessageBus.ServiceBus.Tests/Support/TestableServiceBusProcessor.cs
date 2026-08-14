using Azure.Messaging.ServiceBus;

using System.Threading.Tasks;

namespace Up4All.Framework.MessageBus.ServiceBus.Tests.Support
{
    /// <summary>
    /// ServiceBusProcessor only exposes its ProcessMessageAsync/ProcessErrorAsync events for
    /// subscription (+=); raising them is only possible through OnProcessMessageAsync/
    /// OnProcessErrorAsync, which are protected. This subclass exposes them publicly so tests
    /// can drive the full RegisterHandleMessageAsync(...) pipeline without a live broker.
    /// </summary>
    internal class TestableServiceBusProcessor : ServiceBusProcessor
    {
        public Task RaiseProcessMessageAsync(ProcessMessageEventArgs args) => OnProcessMessageAsync(args);

        public Task RaiseProcessErrorAsync(ProcessErrorEventArgs args) => OnProcessErrorAsync(args);
    }
}
