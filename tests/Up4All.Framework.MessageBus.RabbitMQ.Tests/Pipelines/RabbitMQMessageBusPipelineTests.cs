using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;
using Up4All.Framework.MessageBus.RabbitMQ.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Pipelines;
using RabbitMQStandaloneStreamClientBase = Up4All.Framework.MessageBus.Abstractions.MessageBusStandaloneStreamClient;
using RabbitMQOffsetType = Up4All.Framework.MessageBus.RabbitMQ.Consts.OffsetType;

// Services resolved from these containers are real RabbitMQ client instances whose
// Connection/Channel are never assigned (no broker call happens just from DI resolution).
// Their base class finalizer would call Connection.CloseAsync() and crash the test host
// with a NullReferenceException on the finalizer thread, so every resolved instance below
// has its finalizer suppressed via SuppressFinalizeIfDisposable.

// AddOptions() (called by the RabbitMQMessageBusPipeline constructor) registers the
// framework's own IOptions<T> machinery, which would shadow a plain AddSingleton<IOptions<T>>
// registered *before* it (DI resolves the *last* registration for a given service type).
// All helpers below therefore construct the pipeline first and only add the IOptions<T>
// stand-in afterwards, so it is the one actually resolved.

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests.Pipelines
{
    [TestFixture]
    public class RabbitMQMessageBusPipelineTests
    {
        private static RabbitMQMessageBusPipeline CreatePipeline(out ServiceCollection services, RabbitMQMessageBusOptions? options = null)
        {
            services = new ServiceCollection();
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

            var pipeline = new RabbitMQMessageBusPipeline(services, "MessageBusOptions");

            services.AddSingleton<IOptions<RabbitMQMessageBusOptions>>(Microsoft.Extensions.Options.Options.Create(options ?? new RabbitMQMessageBusOptions
            {
                ConnectionString = "amqp://localhost",
                QueueName = "q",
                TopicName = "t",
                StreamName = "s"
            }));

            return pipeline;
        }

        [Test]
        public void Constructor_ExposesQueuesStreamsAndProducersSubPipelines()
        {
            var pipeline = CreatePipeline(out var services);

            Assert.That(pipeline.Queues, Is.Not.Null);
            Assert.That(pipeline.Streams, Is.Not.Null);
            Assert.That(pipeline.Producers, Is.Not.Null);
            Assert.That(pipeline.Services, Is.SameAs(services));
            Assert.That(pipeline.ConfigurationBindKey, Is.EqualTo("MessageBusOptions"));
        }

        [Test]
        public void Validate_NothingConfigured_Throws()
        {
            var pipeline = CreatePipeline(out _);

            Assert.Throws<ArgumentException>(() => pipeline.Validate());
        }

        [Test]
        public void Validate_ProducerAndHandlersConfigured_DoesNotThrow()
        {
            var pipeline = CreatePipeline(out _);

            pipeline.Producers.AddPublisher();
            pipeline.Queues.ListenQueue().AddHandler(_ => new NoopHandler());
            pipeline.Streams.ListenStreamQueue().AddHandler(_ => new NoopHandler());

            Assert.DoesNotThrow(() => pipeline.Validate());
        }
    }

    [TestFixture]
    public class RabbitMQMessageBusPublisherPipelineTests
    {
        private static (ServiceCollection services, RabbitMQMessageBusPipeline main) CreateMain()
        {
            var services = new ServiceCollection();
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
            var main = new RabbitMQMessageBusPipeline(services, "MessageBusOptions");
            services.AddSingleton<IOptions<RabbitMQMessageBusOptions>>(Microsoft.Extensions.Options.Options.Create(new RabbitMQMessageBusOptions
            {
                ConnectionString = "amqp://localhost",
                TopicName = "topic-1"
            }));
            return (services, main);
        }

        [Test]
        public void AddPublisher_OptionsBased_RegistersPublisherResolvableFromOptions()
        {
            var (services, main) = CreateMain();

            main.Producers.AddPublisher();

            var provider = services.BuildServiceProvider();
            var publisher = provider.GetRequiredService<IMessageBusPublisherAsync>().SuppressFinalizer();

            Assert.That(publisher.TopicName, Is.EqualTo("topic-1"));
        }

        [Test]
        public void AddPublisher_ConnectionStringBased_RegistersPublisher()
        {
            var (services, main) = CreateMain();

            main.Producers.AddPublisher("amqp://localhost", "queue-1");

            var provider = services.BuildServiceProvider();
            var publisher = provider.GetRequiredService<IMessageBusPublisherAsync>().SuppressFinalizer();

            Assert.That(publisher, Is.Not.Null);
        }

        [Test]
        public void AddPublisher_Keyed_RegistersKeyedPublisher()
        {
            var (services, main) = CreateMain();

            // Overload resolution would otherwise prefer the (string, string, ...) overload
            // over the (object serviceKey, ...) one, so the key is cast explicitly.
            main.Producers.AddPublisher((object)"key-1", "amqp://localhost", "queue-1");

            var provider = services.BuildServiceProvider();
            var publisher = provider.GetRequiredKeyedService<IMessageBusPublisherAsync>("key-1").SuppressFinalizer();

            Assert.That(publisher, Is.Not.Null);
        }

        [Test]
        public void AddPublisher_InvokesDeclareBuilderCallback()
        {
            var (services, main) = CreateMain();
            var invoked = false;

            main.Producers.AddPublisher(exchangeDeclareBuilder: (_, _, _) => invoked = true);

            services.BuildServiceProvider().GetRequiredService<IMessageBusPublisherAsync>().SuppressFinalizer();

            Assert.That(invoked, Is.True);
        }
    }

    [TestFixture]
    public class RabbitMQMessageBusQueuePipelineTests
    {
        private static (ServiceCollection services, RabbitMQMessageBusPipeline main) CreateMain()
        {
            var services = new ServiceCollection();
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
            var main = new RabbitMQMessageBusPipeline(services, "MessageBusOptions");
            services.AddSingleton<IOptions<RabbitMQMessageBusOptions>>(Microsoft.Extensions.Options.Options.Create(new RabbitMQMessageBusOptions
            {
                ConnectionString = "amqp://localhost",
                QueueName = "queue-1"
            }));
            return (services, main);
        }

        [Test]
        public void ListenQueue_OptionsBased_RegistersConsumerResolvableFromOptions()
        {
            var (services, main) = CreateMain();

            main.Queues.ListenQueue();

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredService<IMessageBusAsyncConsumer>().SuppressFinalizer();

            Assert.That(consumer.EntityPath, Is.EqualTo("queue-1"));
        }

        [Test]
        public void ListenQueue_ConnectionStringBased_RegistersConsumer()
        {
            var (services, main) = CreateMain();

            main.Queues.ListenQueue("amqp://localhost", "queue-2");

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredService<IMessageBusAsyncConsumer>().SuppressFinalizer();

            Assert.That(consumer.EntityPath, Is.EqualTo("queue-2"));
        }

        [Test]
        public void ListenQueue_Keyed_RegistersKeyedConsumer()
        {
            var (services, main) = CreateMain();

            main.Queues.ListenQueue("key-1", "amqp://localhost", "queue-2");

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredKeyedService<IMessageBusAsyncConsumer>("key-1").SuppressFinalizer();

            Assert.That(consumer, Is.Not.Null);
        }

        [Test]
        public void AddHandler_RegistersHostedService()
        {
            var (services, main) = CreateMain();

            main.Queues.ListenQueue().AddHandler(_ => new NoopHandler());

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<IMessageBusAsyncConsumer>().SuppressFinalizer();
            var hostedServices = provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();

            Assert.That(hostedServices.Any(), Is.True);
        }
    }

    [TestFixture]
    public class RabbitMQMessageBusStreamQueuePipelineTests
    {
        private static (ServiceCollection services, RabbitMQMessageBusPipeline main) CreateMain()
        {
            var services = new ServiceCollection();
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
            var main = new RabbitMQMessageBusPipeline(services, "MessageBusOptions");
            services.AddSingleton<IOptions<RabbitMQMessageBusOptions>>(Microsoft.Extensions.Options.Options.Create(new RabbitMQMessageBusOptions
            {
                ConnectionString = "amqp://localhost",
                StreamName = "stream-1"
            }));
            return (services, main);
        }

        [Test]
        public void ListenStreamQueue_OptionsBased_RegistersConsumerResolvableFromOptions()
        {
            var (services, main) = CreateMain();

            main.Streams.ListenStreamQueue();

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredService<IMessageBusAsyncConsumer>().SuppressFinalizer();

            Assert.That(consumer.EntityPath, Is.EqualTo("stream-1"));
        }

        private static object? GetOffset(object consumer)
        {
            var offsetProperty = typeof(RabbitMQStandaloneStreamClientBase)
                .GetProperty("Offset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return offsetProperty!.GetValue(consumer);
        }

        [Test]
        public void ListenStreamQueue_OptionsBased_NoOffsetProvided_DefaultsToNext()
        {
            // Regression test: the constructor call inside this overload used to pass the
            // StreamDeclareOptions positionally into the "offset" parameter (there was no
            // offset parameter at all), silently corrupting both the offset and the declare
            // options. See RabbitMQMessageBusStreamQueuePipeline.ListenStreamQueue().
            var (services, main) = CreateMain();

            main.Streams.ListenStreamQueue();

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredService<IMessageBusAsyncConsumer>().SuppressFinalizer();

            Assert.That(GetOffset(consumer), Is.EqualTo(RabbitMQOffsetType.Next));
        }

        [Test]
        public void ListenStreamQueue_OptionsBased_ExplicitOffset_IsUsedAsIs()
        {
            var (services, main) = CreateMain();

            main.Streams.ListenStreamQueue(offset: RabbitMQOffsetType.First);

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredService<IMessageBusAsyncConsumer>().SuppressFinalizer();

            Assert.That(GetOffset(consumer), Is.EqualTo(RabbitMQOffsetType.First));
        }

        [Test]
        public void ListenStreamQueue_ConnectionStringBased_RegistersConsumer()
        {
            var (services, main) = CreateMain();

            main.Streams.ListenStreamQueue("amqp://localhost", "stream-2", offset: "first");

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredService<IMessageBusAsyncConsumer>().SuppressFinalizer();

            Assert.That(consumer.EntityPath, Is.EqualTo("stream-2"));
        }

        [Test]
        public void ListenStreamQueue_Keyed_RegistersKeyedConsumer()
        {
            var (services, main) = CreateMain();

            main.Streams.ListenStreamQueue("key-1", "amqp://localhost", "stream-2", offset: "first");

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetRequiredKeyedService<IMessageBusAsyncConsumer>("key-1").SuppressFinalizer();

            Assert.That(consumer, Is.Not.Null);
        }

        [Test]
        public void AddHandler_RegistersHostedService()
        {
            var (services, main) = CreateMain();

            main.Streams.ListenStreamQueue().AddHandler(_ => new NoopHandler());

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<IMessageBusAsyncConsumer>().SuppressFinalizer();
            var hostedServices = provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();

            Assert.That(hostedServices.Any(), Is.True);
        }
    }

    [TestFixture]
    public class IoCExtensionsTests
    {
        [Test]
        public void AddRabbitMQMessageBus_ReturnsConfiguredPipeline()
        {
            var services = new ServiceCollection();

            var pipeline = services.AddRabbitMQMessageBus("MyBus");

            Assert.That(pipeline.Services, Is.SameAs(services));
            Assert.That(pipeline.ConfigurationBindKey, Is.EqualTo("MyBus"));
        }

        [Test]
        public void AddRabbitMQMessageBus_DefaultConfigurationKey_IsMessageBusOptions()
        {
            var services = new ServiceCollection();

            var pipeline = services.AddRabbitMQMessageBus();

            Assert.That(pipeline.ConfigurationBindKey, Is.EqualTo("MessageBusOptions"));
        }
    }

    internal class NoopHandler : Abstractions.Handlers.IMessageBusMessageHandler
    {
        public Task OnErrorAsync(Exception exception, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task OnMessageReceivedAsync(string entityPath, Abstractions.Messages.ReceivedMessage message, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    internal static class TestGcHelper
    {
        /// <summary>
        /// Prevents a client resolved via DI (with no real Connection/Channel assigned)
        /// from crashing the test host process when the GC eventually finalizes it.
        /// </summary>
        public static T SuppressFinalizer<T>(this T instance)
        {
            if (instance is not null)
                GC.SuppressFinalize(instance);
            return instance;
        }
    }
}
