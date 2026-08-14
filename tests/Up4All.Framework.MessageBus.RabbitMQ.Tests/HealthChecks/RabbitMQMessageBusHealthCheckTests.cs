using Microsoft.Extensions.Diagnostics.HealthChecks;

using Moq;

using RabbitMQ.Client;

using System.Threading.Tasks;

using Up4All.Framework.MessageBus.RabbitMQ.HealthChecks;

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests.HealthChecks
{
    [TestFixture]
    public class RabbitMQMessageBusHealthCheckTests
    {
        [Test]
        public async Task CheckHealthAsync_ConnectionOpen_ReturnsHealthy()
        {
            var connectionMock = new Mock<IConnection>();
            connectionMock.SetupGet(c => c.IsOpen).Returns(true);
            var clientMock = new Mock<IRabbitMQClient>();
            clientMock.SetupGet(c => c.Connection).Returns(connectionMock.Object);

            var healthCheck = new RabbitMQMessageBusHealthCheck(() => clientMock.Object);

            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
        }

        [Test]
        public async Task CheckHealthAsync_ConnectionClosed_ReturnsUnhealthy()
        {
            var connectionMock = new Mock<IConnection>();
            connectionMock.SetupGet(c => c.IsOpen).Returns(false);
            var clientMock = new Mock<IRabbitMQClient>();
            clientMock.SetupGet(c => c.Connection).Returns(connectionMock.Object);

            var healthCheck = new RabbitMQMessageBusHealthCheck(() => clientMock.Object);

            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
        }

        [Test]
        public async Task CheckHealthAsync_NullClient_ReturnsUnhealthy()
        {
            var healthCheck = new RabbitMQMessageBusHealthCheck(() => null);

            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
        }

        [Test]
        public async Task CheckHealthAsync_NullConnection_ReturnsUnhealthy()
        {
            var clientMock = new Mock<IRabbitMQClient>();
            clientMock.SetupGet(c => c.Connection).Returns((IConnection)null!);

            var healthCheck = new RabbitMQMessageBusHealthCheck(() => clientMock.Object);

            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
        }
    }
}
