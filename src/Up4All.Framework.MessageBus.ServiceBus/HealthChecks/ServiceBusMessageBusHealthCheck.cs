using Microsoft.Extensions.Diagnostics.HealthChecks;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Up4All.Framework.MessageBus.ServiceBus.HealthChecks
{
    /// <summary>
    /// Reports whether an already-established Azure Service Bus client's connection is open.
    /// Only checks ServiceBusClient.IsClosed - no network round trip (e.g. no peek/receive
    /// call) is made. This is a weak signal (IsClosed only flips when Close/Dispose was
    /// explicitly called or a fatal error already surfaced elsewhere; a client can look "open"
    /// while transiently unable to reach the broker), but it is cheap enough to poll
    /// frequently and matches how the library already tracks connection state.
    /// </summary>
    public class ServiceBusMessageBusHealthCheck(Func<IServiceBusClient?> clientAccessor) : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var client = clientAccessor();
            var isOpen = client?.Client is not null && !client.Client.IsClosed;

            var result = isOpen
                ? HealthCheckResult.Healthy("Service Bus client connection is open.")
                : HealthCheckResult.Unhealthy("Service Bus client is closed or not available.");

            return Task.FromResult(result);
        }
    }
}
