using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Moq;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using RabbitMQ.Client;

using System.Linq;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.RabbitMQ.Extensions;

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests.Extensions
{
    [TestFixture]
    public class IoCExtensionsTests
    {
        [Test]
        public void AddRabbitMQMessageBus_DefaultConfigurationKey_ReturnsPipeline()
        {
            var services = new ServiceCollection();

            var pipeline = services.AddRabbitMQMessageBus();

            Assert.That(pipeline.Services, Is.SameAs(services));
            Assert.That(pipeline.ConfigurationBindKey, Is.EqualTo("MessageBusOptions"));
        }

        [Test]
        public void AddRabbitMQMessageBus_CustomConfigurationKey_ReturnsPipeline()
        {
            var services = new ServiceCollection();

            var pipeline = services.AddRabbitMQMessageBus("CustomKey");

            Assert.That(pipeline.ConfigurationBindKey, Is.EqualTo("CustomKey"));
        }

        [Test]
        public void AddOpenTelemetryForRabbitMQMessageBus_RegistersActivitySourceAndBuildsProvider()
        {
            using var provider = Sdk.CreateTracerProviderBuilder()
                .AddOpenTelemetryForRabbitMQMessageBus()
                .Build();

            Assert.That(provider, Is.Not.Null);
        }

        [Test]
        public void AddOpenTelemetryForRabbitMQMessageBus_ReturnsSameBuilderForChaining()
        {
            var builder = Sdk.CreateTracerProviderBuilder();

            var result = builder.AddOpenTelemetryForRabbitMQMessageBus();

            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void AddOpenTelemetryForRabbitMQMessageBus_DoesNotOverwriteHostServiceName()
        {
            // Regression test: this used to call .SetResourceBuilder(...) with the library's
            // own ActivitySource name, silently replacing whatever service.name the host
            // application had already configured.
            using var provider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("my-app"))
                .AddOpenTelemetryForRabbitMQMessageBus()
                .Build();

            var resource = provider!.GetResource();
            var serviceName = resource.Attributes.First(a => a.Key == "service.name").Value;

            Assert.That(serviceName, Is.EqualTo("my-app"));
        }

        [Test]
        public void AddOpenTelemetryMetricsForRabbitMQMessageBus_RegistersMeterAndBuildsProvider()
        {
            using var provider = Sdk.CreateMeterProviderBuilder()
                .AddOpenTelemetryMetricsForRabbitMQMessageBus()
                .Build();

            Assert.That(provider, Is.Not.Null);
        }

        [Test]
        public void AddOpenTelemetryMetricsForRabbitMQMessageBus_ReturnsSameBuilderForChaining()
        {
            var builder = Sdk.CreateMeterProviderBuilder();

            var result = builder.AddOpenTelemetryMetricsForRabbitMQMessageBus();

            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public async Task AddRabbitMQMessageBusHealthCheck_RegistersCheckThatReflectsClientState()
        {
            var connectionMock = new Mock<IConnection>();
            connectionMock.SetupGet(c => c.IsOpen).Returns(true);
            var clientMock = new Mock<IRabbitMQClient>();
            clientMock.SetupGet(c => c.Connection).Returns(connectionMock.Object);

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHealthChecks()
                .AddRabbitMQMessageBusHealthCheck(_ => clientMock.Object);

            using var provider = services.BuildServiceProvider();
            var healthCheckService = provider.GetRequiredService<HealthCheckService>();

            var report = await healthCheckService.CheckHealthAsync();

            Assert.That(report.Status, Is.EqualTo(HealthStatus.Healthy));
            Assert.That(report.Entries.ContainsKey("rabbitmq-messagebus"), Is.True);
        }
    }
}
