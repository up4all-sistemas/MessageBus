using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using Moq;

using System.Threading.Tasks;

using Up4All.Framework.MessageBus.ServiceBus.HealthChecks;

namespace Up4All.Framework.MessageBus.ServiceBus.Tests.HealthChecks
{
    [TestFixture]
    public class ServiceBusMessageBusHealthCheckTests
    {
        [Test]
        public async Task CheckHealthAsync_ClientOpen_ReturnsHealthy()
        {
            var sbClient = new ServiceBusClient(
                "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ZmFrZWtleQ==");
            var clientMock = new Mock<IServiceBusClient>();
            clientMock.SetupGet(c => c.Client).Returns(sbClient);

            var healthCheck = new ServiceBusMessageBusHealthCheck(() => clientMock.Object);

            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));

            await sbClient.DisposeAsync();
        }

        [Test]
        public async Task CheckHealthAsync_ClientClosed_ReturnsUnhealthy()
        {
            var sbClient = new ServiceBusClient(
                "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ZmFrZWtleQ==");
            await sbClient.DisposeAsync();

            var clientMock = new Mock<IServiceBusClient>();
            clientMock.SetupGet(c => c.Client).Returns(sbClient);

            var healthCheck = new ServiceBusMessageBusHealthCheck(() => clientMock.Object);

            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
        }

        [Test]
        public async Task CheckHealthAsync_NullClient_ReturnsUnhealthy()
        {
            var healthCheck = new ServiceBusMessageBusHealthCheck(() => null);

            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
        }
    }
}
