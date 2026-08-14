using System;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Options;

using Up4All.Framework.MessageBus.Tests.Support;

namespace Up4All.Framework.MessageBus.Tests.Pipelines
{
    [TestFixture]
    public class MessageBusPipelineTests
    {
        [Test]
        public void Constructor_SetsServicesAndConfigurationBindKey()
        {
            var services = new ServiceCollection();

            var pipeline = new FakePipeline(services, "MyBus");

            Assert.That(pipeline.Services, Is.SameAs(services));
            Assert.That(pipeline.ConfigurationBindKey, Is.EqualTo("MyBus"));
            Assert.That(pipeline.OptionsBuilder, Is.Null);
        }

        [Test]
        public void AddOptions_SetsOptionsBuilder()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");

            pipeline.CallAddOptions();

            Assert.That(pipeline.OptionsBuilder, Is.Not.Null);
        }

        [Test]
        public void AddOptions_BindsConfigurationSection()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                [
                    new("MyBus:ConnectionString", "amqp://localhost"),
                    new("MyBus:QueueName", "queue-1")
                ])
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            var pipeline = new FakePipeline(services, "MyBus");
            pipeline.CallAddOptions();

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<MessageBusOptions>>().Value;

            Assert.That(options.ConnectionString, Is.EqualTo("amqp://localhost"));
            Assert.That(options.QueueName, Is.EqualTo("queue-1"));
        }

        [Test]
        public void Validate_InvokesValidateOnEachRegisteredInnerPipeline()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var inner1 = new FakeInnerPipelineBuilder();
            var inner2 = new FakeInnerPipelineBuilder();
            pipeline.AddInnerPipeline(inner1);
            pipeline.AddInnerPipeline(inner2);

            pipeline.Validate();

            Assert.That(inner1.ValidateCalled, Is.True);
            Assert.That(inner2.ValidateCalled, Is.True);
        }
    }

    [TestFixture]
    public class MessageBusFlowPipelineTests
    {
        [Test]
        public void Then_ReturnsMainPipeline()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var publisher = new FakePublisherPipeline(pipeline);

            var result = publisher.Then();

            Assert.That(result, Is.SameAs(pipeline));
        }
    }

    [TestFixture]
    public class MessageBusPublisherPipelineTests
    {
        [Test]
        public void Validate_WhenPublisherNotDefined_Throws()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var publisher = new FakePublisherPipeline(pipeline);

            Assert.Throws<ArgumentException>(() => publisher.Validate());
        }

        [Test]
        public void Validate_WhenPublisherDefined_DoesNotThrow()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var publisher = new FakePublisherPipeline(pipeline);

            publisher.MarkPublisherDefined();

            Assert.DoesNotThrow(() => publisher.Validate());
        }
    }

    [TestFixture]
    public class MessageBusConsumerPipelineTests
    {
        [Test]
        public void Validate_WhenHandlerNotDefined_Throws()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var consumerPipeline = new FakeConsumerPipeline(pipeline);

            Assert.Throws<ArgumentException>(() => consumerPipeline.Validate());
        }

        [Test]
        public void AddHandler_RegistersHandlerAndMarksDefined()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var consumerPipeline = new FakeConsumerPipeline(pipeline);

            consumerPipeline.AddHandler<FakeMessageHandler>();

            var provider = services.BuildServiceProvider();
            var handler = provider.GetService<IMessageBusMessageHandler>();

            Assert.That(handler, Is.InstanceOf<FakeMessageHandler>());
            Assert.DoesNotThrow(() => consumerPipeline.Validate());
        }

        [Test]
        public void AddHandler_WithFactory_RegistersHandlerAndMarksDefined()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var consumerPipeline = new FakeConsumerPipeline(pipeline);
            var instance = new FakeMessageHandler();

            consumerPipeline.AddHandler(_ => instance);

            var provider = services.BuildServiceProvider();
            var handler = provider.GetService<IMessageBusMessageHandler>();

            Assert.That(handler, Is.SameAs(instance));
        }

        [Test]
        public void AddDefaultHostedService_RegistersHostedService()
        {
            var services = new ServiceCollection();
            var pipeline = new FakePipeline(services, "MyBus");
            var consumerPipeline = new FakeConsumerPipeline(pipeline);

            consumerPipeline.AddDefaultHostedService();

            var provider = services.BuildServiceProvider();
            var hostedServices = provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();

            Assert.That(hostedServices.OfType<FakeHostedConsumer>().Any(), Is.True);
        }

        [Test]
        public void AddHostedService_RemovesPreviouslyRegisteredDefaultConsumer()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IMessageDefaultConsumer, FakeHostedConsumer>();
            var pipeline = new FakePipeline(services, "MyBus");
            var consumerPipeline = new FakeConsumerPipeline(pipeline);

            consumerPipeline.AddHostedService<FakeHostedConsumer>();

            var registeredCount = services.Count(sd => sd.ServiceType == typeof(IMessageDefaultConsumer));
            Assert.That(registeredCount, Is.EqualTo(0));
        }
    }
}
