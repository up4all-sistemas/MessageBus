using Moq;

using RabbitMQ.Client;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Consts;
using Up4All.Framework.MessageBus.RabbitMQ.Consumers;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;
using Up4All.Framework.MessageBus.RabbitMQ.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Tests.Support;

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests.Extensions
{
    [TestFixture]
    public class RabbitMQClientExtensionsTests
    {
        private static Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> NoopHandler
            => (_, _) => Task.FromResult(MessageReceivedStatus.Completed);

        private static Func<Exception, CancellationToken, Task> NoopErrorHandler
            => (_, _) => Task.CompletedTask;

        [Test]
        public void ActivitySource_IsASingletonAcrossCalls()
        {
            // Regression test: this used to be a property that built a brand-new
            // ActivitySource (with its own assembly-name/version lookup) on every access.
            var first = RabbitMQClientExtensions.ActivitySource;
            var second = RabbitMQClientExtensions.ActivitySource;

            Assert.That(first, Is.SameAs(second));
        }

        [Test]
        public void Meter_IsASingletonAcrossCalls()
        {
            var first = RabbitMQClientExtensions.Meter;
            var second = RabbitMQClientExtensions.Meter;

            Assert.That(first, Is.SameAs(second));
        }

        [Test]
        public async Task ConfigureAsyncHandler_WithoutOffset_DoesNotAddOffsetArgument()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            var clientMock = new Mock<IRabbitMQClient>();
            clientMock.SetupGet(c => c.Channel).Returns(channelMock.Object);

            IDictionary<string, object?>? capturedArgs = null;
            channelMock.Setup(c => c.BasicConsumeAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()
                    , It.IsAny<IDictionary<string, object?>>(), It.IsAny<IAsyncBasicConsumer>(), It.IsAny<CancellationToken>()))
                .Callback<string, bool, string, bool, bool, IDictionary<string, object?>, IAsyncBasicConsumer, CancellationToken>(
                    (_, _, _, _, _, args, _, _) => capturedArgs = args)
                .ReturnsAsync("tag");

            var receiver = new AsyncQueueMessageReceiver(channelMock.Object, NoopHandler, NoopErrorHandler, null, false, RabbitMQMocks.Logger<AsyncQueueMessageReceiver>());

            await clientMock.Object.ConfigureAsyncHandler("queue-1", receiver, false, CancellationToken.None);

            channelMock.Verify(c => c.BasicQosAsync(0, 1, false, It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(capturedArgs, Is.Not.Null);
            Assert.That(capturedArgs!.ContainsKey(Arguments.StreamOffsetKey), Is.False);
        }

        [Test]
        public async Task ConfigureAsyncHandler_WithOffset_AddsOffsetArgument()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            var clientMock = new Mock<IRabbitMQClient>();
            clientMock.SetupGet(c => c.Channel).Returns(channelMock.Object);

            IDictionary<string, object?>? capturedArgs = null;
            channelMock.Setup(c => c.BasicConsumeAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()
                    , It.IsAny<IDictionary<string, object?>>(), It.IsAny<IAsyncBasicConsumer>(), It.IsAny<CancellationToken>()))
                .Callback<string, bool, string, bool, bool, IDictionary<string, object?>, IAsyncBasicConsumer, CancellationToken>(
                    (_, _, _, _, _, args, _, _) => capturedArgs = args)
                .ReturnsAsync("tag");

            var receiver = new AsyncQueueMessageReceiver(channelMock.Object, NoopHandler, NoopErrorHandler, null, false, RabbitMQMocks.Logger<AsyncQueueMessageReceiver>());

            await clientMock.Object.ConfigureAsyncHandler("stream-1", receiver, false, CancellationToken.None, offset: "first");

            Assert.That(capturedArgs, Is.Not.Null);
            Assert.That(capturedArgs![Arguments.StreamOffsetKey], Is.EqualTo("first"));
        }

        [Test]
        public async Task ConfigureAsyncHandlerGeneric_WithOffset_AddsOffsetArgument()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            var clientMock = new Mock<IRabbitMQClient>();
            clientMock.SetupGet(c => c.Channel).Returns(channelMock.Object);

            var receiver = new AsyncQueueMessageReceiverForModel<string>(channelMock.Object
                , (_, _) => Task.FromResult(MessageReceivedStatus.Completed), NoopErrorHandler, null, false
                , RabbitMQMocks.Logger<AsyncQueueMessageReceiverForModel<string>>());

            await clientMock.Object.ConfigureAsyncHandler("queue-1", receiver, false, CancellationToken.None, offset: 5);

            channelMock.Verify(c => c.BasicConsumeAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()
                    , It.Is<IDictionary<string, object?>>(d => d.ContainsKey(Arguments.StreamOffsetKey) && (int)d[Arguments.StreamOffsetKey]! == 5)
                    , It.IsAny<IAsyncBasicConsumer>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendMessageAsync_PublishesWithDefaultRoutingKeyAndPersistence()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            BasicProperties? captured = null;
            channelMock.Setup(c => c.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BasicProperties>()
                    , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, bool, BasicProperties, System.ReadOnlyMemory<byte>, CancellationToken>(
                    (_, _, _, props, _, _) => captured = props)
                .Returns(default(ValueTask));

            var message = new MessageBusMessage();
            message.AddBody("hello");

            await channelMock.Object.SendMessageAsync(RabbitMQMocks.Logger<RabbitMQClientExtensionsTests>(), "my-topic", "my-queue", message);

            channelMock.Verify(c => c.BasicPublishAsync("my-topic", "my-queue", false, It.IsAny<BasicProperties>()
                , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.Persistent, Is.True);
            Assert.That(captured.DeliveryMode, Is.EqualTo(DeliveryModes.Persistent));
            Assert.That(captured.Headers, Is.Not.Null);
        }

        [Test]
        public async Task SendMessageAsync_TagsActivityWithPublishOperationType()
        {
            // "publish" is the value defined by the OpenTelemetry messaging semantic
            // conventions for a producer-side operation; "send" is not a recognized value.
            System.Diagnostics.Activity? stopped = null;
            using var listener = new System.Diagnostics.ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref System.Diagnostics.ActivityCreationOptions<System.Diagnostics.ActivityContext> options) => System.Diagnostics.ActivitySamplingResult.AllData,
                ActivityStopped = a => stopped = a
            };
            System.Diagnostics.ActivitySource.AddActivityListener(listener);

            var channelMock = RabbitMQMocks.CreateChannel();
            var message = new MessageBusMessage();
            message.AddBody("hello");

            await channelMock.Object.SendMessageAsync(RabbitMQMocks.Logger<RabbitMQClientExtensionsTests>(), "my-topic", "my-queue", message);

            Assert.That(stopped, Is.Not.Null);
            Assert.That(stopped!.GetTagItem("messaging.operation.type"), Is.EqualTo("publish"));
        }

        [Test]
        public async Task SendMessageAsync_Success_RecordsSentCounterAndDurationWithoutErrorType()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            var message = new MessageBusMessage();
            message.AddBody("hello");

            var measurements = MetricsTestHelper.CaptureMeasurements(RabbitMQClientExtensions.Meter, () =>
                channelMock.Object.SendMessageAsync(RabbitMQMocks.Logger<RabbitMQClientExtensionsTests>(), "my-topic", "my-queue", message).GetAwaiter().GetResult());

            var sent = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.SentMessagesInstrumentName);
            Assert.That(sent.Value, Is.EqualTo(1L));
            Assert.That(sent.Tags.Any(t => t.Key == "error.type"), Is.False);

            var duration = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.OperationDurationInstrumentName);
            Assert.That(duration.Tags.First(t => t.Key == "messaging.operation.type").Value, Is.EqualTo("publish"));
            Assert.That(duration.Tags.Any(t => t.Key == "error.type"), Is.False);
        }

        [Test]
        public void SendMessageAsync_PublishThrows_RecordsDurationWithErrorTypeAndDoesNotRecordSentCounter()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            channelMock.Setup(c => c.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BasicProperties>()
                    , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var message = new MessageBusMessage();
            message.AddBody("hello");

            // The finally block records the duration measurement before the exception
            // finishes propagating, so the listener callback still fires even though the
            // outer call ends up throwing - capture into a list from outside, not through
            // MetricsTestHelper's return value.
            var measurements = new List<CapturedMeasurement>();
            using var listener = new System.Diagnostics.Metrics.MeterListener();
            listener.InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter == RabbitMQClientExtensions.Meter)
                    l.EnableMeasurementEvents(instrument);
            };
            listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
                measurements.Add(new CapturedMeasurement(instrument.Name, value, tags.ToArray())));
            listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
                measurements.Add(new CapturedMeasurement(instrument.Name, value, tags.ToArray())));
            listener.Start();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                channelMock.Object.SendMessageAsync(RabbitMQMocks.Logger<RabbitMQClientExtensionsTests>(), "my-topic", "my-queue", message));

            Assert.That(measurements.Any(m => m.InstrumentName == OpenTelemetryMetricsExtensions.SentMessagesInstrumentName), Is.False);

            var duration = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.OperationDurationInstrumentName);
            Assert.That(duration.Tags.First(t => t.Key == "error.type").Value, Is.EqualTo(nameof(InvalidOperationException)));
        }

        [Test]
        public async Task SendMessageAsync_UsesMessageRoutingKey_WhenPresent()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            var message = new MessageBusMessage();
            message.AddBody("hello");
            message.AddRoutingKey("custom-rk");

            await channelMock.Object.SendMessageAsync(RabbitMQMocks.Logger<RabbitMQClientExtensionsTests>(), "my-topic", "my-queue", message);

            channelMock.Verify(c => c.BasicPublishAsync("my-topic", "custom-rk", false, It.IsAny<BasicProperties>()
                , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendMessageAsync_IsPersistentUserProperty_OverridesPersistentParameter()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            BasicProperties? captured = null;
            channelMock.Setup(c => c.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BasicProperties>()
                    , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, bool, BasicProperties, System.ReadOnlyMemory<byte>, CancellationToken>(
                    (_, _, _, props, _, _) => captured = props)
                .Returns(default(ValueTask));

            // MessageBusMessageExtensions.IsPersistent(true) is the only way to force the
            // "mb-is-persistent" user property to a concrete value (false just removes it),
            // so it is used here to override the method's persistent:false default to true.
            var message = new MessageBusMessage();
            message.AddBody("hello");
            message.IsPersistent(true);

            await channelMock.Object.SendMessageAsync(RabbitMQMocks.Logger<RabbitMQClientExtensionsTests>(), "topic", "queue", message, persistent: false);

            Assert.That(captured!.Persistent, Is.True);
            Assert.That(captured.DeliveryMode, Is.EqualTo(DeliveryModes.Persistent));
        }

        [Test]
        public async Task SendMessageAsync_WithCorrelationId_SetsBasicPropertiesCorrelationId()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            BasicProperties? captured = null;
            channelMock.Setup(c => c.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BasicProperties>()
                    , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, bool, BasicProperties, System.ReadOnlyMemory<byte>, CancellationToken>(
                    (_, _, _, props, _, _) => captured = props)
                .Returns(default(ValueTask));

            var message = new MessageBusMessage();
            message.AddBody("hello");
            var correlationId = Guid.NewGuid();
            message.SetCorrelationId(correlationId);

            await channelMock.Object.SendMessageAsync(RabbitMQMocks.Logger<RabbitMQClientExtensionsTests>(), "topic", "queue", message);

            Assert.That(captured!.CorrelationId, Is.EqualTo(correlationId.ToString()));
        }

        [Test]
        public void GetConnectionAsync_InvalidConnectionString_ThrowsCapturedException()
        {
            var clientMock = new Mock<IRabbitMQClient>();
            clientMock.SetupGet(c => c.Connection).Returns((IConnection)null!);

            Assert.ThrowsAsync<UriFormatException>(() =>
                clientMock.Object.GetConnectionAsync("not a valid uri###", 0, CancellationToken.None));
        }

        [Test]
        public async Task GetConnectionAsync_ConnectionAlreadySet_ReturnsExistingWithoutReconnecting()
        {
            var existingConnection = new Mock<IConnection>().Object;
            var clientMock = new Mock<IRabbitMQClient>();
            clientMock.SetupGet(c => c.Connection).Returns(existingConnection);

            var result = await clientMock.Object.GetConnectionAsync("amqp://localhost", 3, CancellationToken.None);

            Assert.That(result, Is.SameAs(existingConnection));
        }

        [Test]
        public async Task CreateChannelAsync_ChannelAlreadySet_ReturnsExistingWithoutCreatingNew()
        {
            var existingChannel = new Mock<IChannel>().Object;
            var connectionMock = new Mock<IConnection>();
            var clientMock = new Mock<IRabbitMQClient>();
            clientMock.SetupGet(c => c.Channel).Returns(existingChannel);
            clientMock.SetupGet(c => c.Connection).Returns(connectionMock.Object);

            var result = await clientMock.Object.CreateChannelAsync(CancellationToken.None);

            Assert.That(result, Is.SameAs(existingChannel));
            connectionMock.Verify(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task CreateChannelAsync_ChannelNull_CreatesChannelFromConnection()
        {
            var newChannel = new Mock<IChannel>().Object;
            var connectionMock = RabbitMQMocks.CreateConnection(newChannel);
            var clientMock = new Mock<IRabbitMQClient>();
            clientMock.SetupGet(c => c.Channel).Returns((IChannel)null!);
            clientMock.SetupGet(c => c.Connection).Returns(connectionMock.Object);

            var result = await clientMock.Object.CreateChannelAsync(CancellationToken.None);

            Assert.That(result, Is.SameAs(newChannel));
        }

        [Test]
        public async Task ConfigureQueueDeclareAsync_NullOptions_DoesNothing()
        {
            var channelMock = RabbitMQMocks.CreateChannel();

            await channelMock.Object.ConfigureQueueDeclareAsync("queue", null, CancellationToken.None);

            channelMock.Verify(c => c.QueueDeclareAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()
                , It.IsAny<IDictionary<string, object?>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ConfigureQueueDeclareAsync_AddsQueueTypeArgumentAndDeclares()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            var options = RabbitMQConsts.DefaultQueueDeclareOptions;

            await channelMock.Object.ConfigureQueueDeclareAsync("queue-1", options, CancellationToken.None);

            Assert.That(options.Args["x-queue-type"], Is.EqualTo(QueueType.Classic));
            channelMock.Verify(c => c.QueueDeclareAsync("queue-1", options.Durable, options.Exclusive, options.AutoDelete
                , options.Args, false, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ConfigureQueueDeclareAsync_DoesNotOverrideExistingQueueTypeArgument()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            var options = RabbitMQConsts.DefaultQueueDeclareOptions;
            options.Args["x-queue-type"] = "custom-type";

            await channelMock.Object.ConfigureQueueDeclareAsync("queue-1", options, CancellationToken.None);

            Assert.That(options.Args["x-queue-type"], Is.EqualTo("custom-type"));
        }

        [Test]
        public async Task ConfigureQueueDeclareAsync_WithBindings_BindsEachOne()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            var options = RabbitMQConsts.DefaultQueueDeclareOptions;
            options.AddBinding("ex-1", b => b.RoutingKey = "rk-1");
            options.AddBinding("ex-2", b => { });

            await channelMock.Object.ConfigureQueueDeclareAsync("queue-1", options, CancellationToken.None);

            channelMock.Verify(c => c.QueueBindAsync("queue-1", "ex-1", "rk-1", It.IsAny<IDictionary<string, object?>>(), false, It.IsAny<CancellationToken>()), Times.Once);
            channelMock.Verify(c => c.QueueBindAsync("queue-1", "ex-2", "", It.IsAny<IDictionary<string, object?>>(), false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestCase(true, MessageReceivedStatus.Completed)]
        [TestCase(true, MessageReceivedStatus.Deadletter)]
        [TestCase(true, MessageReceivedStatus.Abandoned)]
        public async Task ProcessMessageAsync_AutoComplete_NeverAcksNacksOrRejects(bool autoComplete, MessageReceivedStatus status)
        {
            var channelMock = RabbitMQMocks.CreateChannel();

            await channelMock.Object.ProcessMessageAsync(1, status, autoComplete, CancellationToken.None);

            channelMock.Verify(c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
            channelMock.Verify(c => c.BasicNackAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
            channelMock.Verify(c => c.BasicRejectAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ProcessMessageAsync_Completed_Acks()
        {
            var channelMock = RabbitMQMocks.CreateChannel();

            await channelMock.Object.ProcessMessageAsync(42, MessageReceivedStatus.Completed, false, CancellationToken.None);

            channelMock.Verify(c => c.BasicAckAsync(42, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ProcessMessageAsync_Deadletter_Nacks()
        {
            var channelMock = RabbitMQMocks.CreateChannel();

            await channelMock.Object.ProcessMessageAsync(42, MessageReceivedStatus.Deadletter, false, CancellationToken.None);

            channelMock.Verify(c => c.BasicNackAsync(42, false, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ProcessMessageAsync_Abandoned_Rejects()
        {
            var channelMock = RabbitMQMocks.CreateChannel();

            await channelMock.Object.ProcessMessageAsync(42, MessageReceivedStatus.Abandoned, false, CancellationToken.None);

            channelMock.Verify(c => c.BasicRejectAsync(42, true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ProcessErrorMessageAsync_AutoCompleteFalse_Nacks()
        {
            var channelMock = RabbitMQMocks.CreateChannel();

            await channelMock.Object.ProcessErrorMessageAsync(7, false, CancellationToken.None);

            channelMock.Verify(c => c.BasicNackAsync(7, false, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ProcessErrorMessageAsync_AutoCompleteTrue_DoesNotNack()
        {
            var channelMock = RabbitMQMocks.CreateChannel();

            await channelMock.Object.ProcessErrorMessageAsync(7, true, CancellationToken.None);

            channelMock.Verify(c => c.BasicNackAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void CreateActivityName_WithChannel_UsesCurrentQueue()
        {
            var channelMock = RabbitMQMocks.CreateChannel(currentQueue: "q1");
            var consumerMock = new Mock<IAsyncBasicConsumer>();
            consumerMock.SetupGet(c => c.Channel).Returns(channelMock.Object);

            var name = consumerMock.Object.CreateActivityName("exchange-1");

            Assert.That(name, Is.EqualTo("exchange-1:q1 receive"));
        }

        [Test]
        public void CreateActivityName_NullChannel_OmitsQueueName()
        {
            var consumerMock = new Mock<IAsyncBasicConsumer>();
            consumerMock.SetupGet(c => c.Channel).Returns((IChannel)null!);

            var name = consumerMock.Object.CreateActivityName("exchange-1");

            Assert.That(name, Is.EqualTo("exchange-1: receive"));
        }

        [Test]
        public void CreateMessageReceivedActivityName_DelegatesToCreateActivityName()
        {
            var channelMock = RabbitMQMocks.CreateChannel(currentQueue: "q2");
            var consumerMock = new Mock<IAsyncBasicConsumer>();
            consumerMock.SetupGet(c => c.Channel).Returns(channelMock.Object);

            var name = consumerMock.Object.CreateMessageReceivedActivityName("exchange-2");

            Assert.That(name, Is.EqualTo("exchange-2:q2 receive"));
        }

        [Test]
        public void CreateMessageReceivedActivity_NullHeaders_ReturnsActivity()
        {
            var channelMock = RabbitMQMocks.CreateChannel(currentQueue: "q3");
            var consumerMock = new Mock<IAsyncBasicConsumer>();
            consumerMock.SetupGet(c => c.Channel).Returns(channelMock.Object);
            var properties = new BasicProperties();

            using var activity = consumerMock.Object.CreateMessageReceivedActivity(properties, "exchange-3", "rk");

            Assert.That(activity, Is.Not.Null);
            Assert.That(activity!.OperationName, Is.EqualTo("exchange-3:q3 receive"));
        }

        [Test]
        public void CreateMessageReceivedActivity_SkipsNullHeaderValues()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            var consumerMock = new Mock<IAsyncBasicConsumer>();
            consumerMock.SetupGet(c => c.Channel).Returns(channelMock.Object);
            var properties = new BasicProperties
            {
                Headers = new Dictionary<string, object?> { { "null-header", null }, { "valid-header", "value" } }
            };

            Assert.DoesNotThrow(() =>
            {
                using var activity = consumerMock.Object.CreateMessageReceivedActivity(properties, "exchange-4", "rk");
            });
        }
    }
}
