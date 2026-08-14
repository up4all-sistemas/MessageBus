using Confluent.Kafka;

using Moq;

using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Kafka.Extensions;
using Up4All.Framework.MessageBus.Kafka.Tests.Support;

namespace Up4All.Framework.MessageBus.Kafka.Tests.Clients
{
    [TestFixture]
    public class KafkaStandaloneGenericSubscriptionAsyncClientTests
    {
        private static ConsumeResult<string, byte[]> CreateConsumeResult(string key, string body) => new()
        {
            Message = new Message<string, byte[]> { Key = key, Value = Encoding.UTF8.GetBytes(body) }
        };

        [Test]
        public void Constructor_ExposesSubscriptionNameAsEntityPath()
        {
            var client = new TestableKafkaSubscriptionClient("localhost:1", "topic-1", "sub-1");

            Assert.That(client.EntityPath, Is.EqualTo("sub-1"));
        }

        [Test]
        public async Task RegisterHandlerAsync_SuccessfulMessage_InvokesHandlerAndCommits()
        {
            var client = new TestableKafkaSubscriptionClient("localhost:1", "topic-1", "sub-1");
            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            client.SetConsumerForTest(consumerMock.Object);

            var cts = new CancellationTokenSource();
            consumerMock.Setup(c => c.Consume(It.IsAny<CancellationToken>()))
                .Returns(() => { cts.Cancel(); return CreateConsumeResult("k1", "payload"); });

            ReceivedMessage? received = null;
            Task<MessageReceivedStatus> Handler(ReceivedMessage msg, CancellationToken ct)
            {
                received = msg;
                return Task.FromResult(MessageReceivedStatus.Completed);
            }

            await client.RegisterHandlerAsync(Handler, (_, _) => Task.CompletedTask, cancellationToken: cts.Token);

            Assert.That(received, Is.Not.Null);
            Assert.That(received!.GetBody(), Is.EqualTo("payload"));
            consumerMock.Verify(c => c.Subscribe("topic-1"), Times.Once);
            consumerMock.Verify(c => c.Commit(), Times.Once);
        }

        [Test]
        public async Task RegisterHandlerAsync_AbandonedResult_DoesNotCommit()
        {
            var client = new TestableKafkaSubscriptionClient("localhost:1", "topic-1", "sub-1");
            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            client.SetConsumerForTest(consumerMock.Object);

            var cts = new CancellationTokenSource();
            consumerMock.Setup(c => c.Consume(It.IsAny<CancellationToken>()))
                .Returns(() => { cts.Cancel(); return CreateConsumeResult("k1", "payload"); });

            await client.RegisterHandlerAsync((_, _) => Task.FromResult(MessageReceivedStatus.Abandoned), (_, _) => Task.CompletedTask, cancellationToken: cts.Token);

            consumerMock.Verify(c => c.Commit(), Times.Never);
        }

        [Test]
        public async Task RegisterHandlerAsync_HandlerThrows_InvokesErrorHandlerAndDoesNotCommit()
        {
            var client = new TestableKafkaSubscriptionClient("localhost:1", "topic-1", "sub-1");
            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            client.SetConsumerForTest(consumerMock.Object);

            var cts = new CancellationTokenSource();
            consumerMock.Setup(c => c.Consume(It.IsAny<CancellationToken>()))
                .Returns(() => { cts.Cancel(); return CreateConsumeResult("k1", "payload"); });

            Exception? captured = null;
            Task<MessageReceivedStatus> Handler(ReceivedMessage msg, CancellationToken ct) => throw new InvalidOperationException("boom");
            Task ErrorHandler(Exception ex, CancellationToken ct) { captured = ex; return Task.CompletedTask; }

            await client.RegisterHandlerAsync(Handler, ErrorHandler, cancellationToken: cts.Token);

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.Message, Is.EqualTo("boom"));
            consumerMock.Verify(c => c.Commit(), Times.Never);
        }

        [Test]
        public async Task RegisterHandlerAsync_SuccessfulMessage_RecordsConsumedCounterAndDurationWithoutErrorType()
        {
            var client = new TestableKafkaSubscriptionClient("localhost:1", "topic-1", "sub-1");
            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            client.SetConsumerForTest(consumerMock.Object);

            var cts = new CancellationTokenSource();
            consumerMock.Setup(c => c.Consume(It.IsAny<CancellationToken>()))
                .Returns(() => { cts.Cancel(); return CreateConsumeResult("k1", "payload"); });

            Task<MessageReceivedStatus> Handler(ReceivedMessage _, CancellationToken __) => Task.FromResult(MessageReceivedStatus.Completed);

            var measurements = MetricsTestHelper.CaptureMeasurements(KafkaExtensions.Meter, () =>
                client.RegisterHandlerAsync(Handler, (_, _) => Task.CompletedTask, cancellationToken: cts.Token).GetAwaiter().GetResult());

            var consumed = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.ConsumedMessagesInstrumentName);
            Assert.That(consumed.Value, Is.EqualTo(1L));
            Assert.That(consumed.Tags.First(t => t.Key == "messaging.operation.type").Value, Is.EqualTo("receive"));
            Assert.That(consumed.Tags.Any(t => t.Key == "error.type"), Is.False);

            var duration = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.OperationDurationInstrumentName);
            Assert.That(duration.Tags.Any(t => t.Key == "error.type"), Is.False);
        }

        [Test]
        public async Task RegisterHandlerAsync_HandlerThrows_RecordsConsumedCounterAndDurationWithErrorType()
        {
            var client = new TestableKafkaSubscriptionClient("localhost:1", "topic-1", "sub-1");
            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            client.SetConsumerForTest(consumerMock.Object);

            var cts = new CancellationTokenSource();
            consumerMock.Setup(c => c.Consume(It.IsAny<CancellationToken>()))
                .Returns(() => { cts.Cancel(); return CreateConsumeResult("k1", "payload"); });

            var exception = new InvalidOperationException("boom");

            var measurements = MetricsTestHelper.CaptureMeasurements(KafkaExtensions.Meter, () =>
                client.RegisterHandlerAsync((_, _) => throw exception, (_, _) => Task.CompletedTask, cancellationToken: cts.Token).GetAwaiter().GetResult());

            var consumed = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.ConsumedMessagesInstrumentName);
            Assert.That(consumed.Tags.First(t => t.Key == "error.type").Value, Is.EqualTo(nameof(InvalidOperationException)));

            var duration = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.OperationDurationInstrumentName);
            Assert.That(duration.Tags.First(t => t.Key == "error.type").Value, Is.EqualTo(nameof(InvalidOperationException)));
        }

        [Test]
        public async Task RegisterHandlerAsync_HandlerThrows_MarksActivityAsErrorAndRecordsException()
        {
            Activity? stopped = null;
            using var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
                ActivityStopped = a => stopped = a
            };
            ActivitySource.AddActivityListener(listener);

            var client = new TestableKafkaSubscriptionClient("localhost:1", "topic-1", "sub-1");
            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            client.SetConsumerForTest(consumerMock.Object);

            var cts = new CancellationTokenSource();
            consumerMock.Setup(c => c.Consume(It.IsAny<CancellationToken>()))
                .Returns(() => { cts.Cancel(); return CreateConsumeResult("k1", "payload"); });

            var exception = new InvalidOperationException("boom");
            await client.RegisterHandlerAsync((_, _) => throw exception, (_, _) => Task.CompletedTask, cancellationToken: cts.Token);

            Assert.That(stopped, Is.Not.Null);
            Assert.That(stopped!.Status, Is.EqualTo(ActivityStatusCode.Error));
            Assert.That(stopped.StatusDescription, Is.EqualTo("boom"));
            Assert.That(stopped.Events.Any(e => e.Name == "exception"), Is.True);
        }

        [Test]
        public async Task RegisterHandlerAsync_SuccessfulMessage_TagsActivityWithPartitionOffsetAndConsumerGroup()
        {
            Activity? stopped = null;
            using var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
                ActivityStopped = a => stopped = a
            };
            ActivitySource.AddActivityListener(listener);

            var client = new TestableKafkaSubscriptionClient("localhost:1", "topic-1", "sub-1");
            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            client.SetConsumerForTest(consumerMock.Object);

            var cts = new CancellationTokenSource();
            var consumeResult = CreateConsumeResult("k1", "payload");
            consumeResult.Partition = new Partition(2);
            consumeResult.Offset = new Offset(42L);

            consumerMock.Setup(c => c.Consume(It.IsAny<CancellationToken>()))
                .Returns(() => { cts.Cancel(); return consumeResult; });

            Task<MessageReceivedStatus> Handler(ReceivedMessage _, CancellationToken __) => Task.FromResult(MessageReceivedStatus.Completed);

            await client.RegisterHandlerAsync(Handler, (_, _) => Task.CompletedTask, cancellationToken: cts.Token);

            // Partition/offset are set as boxed int/long via SetTag(string, object), so they
            // only show up in TagObjects - Activity.Tags (string,string) silently drops any
            // tag whose value isn't already a string.
            Assert.That(stopped, Is.Not.Null);
            Assert.That(stopped!.TagObjects.First(t => t.Key == "messaging.kafka.message.partition").Value, Is.EqualTo(2));
            Assert.That(stopped.TagObjects.First(t => t.Key == "messaging.kafka.message.offset").Value, Is.EqualTo(42L));
            Assert.That(stopped.TagObjects.First(t => t.Key == "messaging.kafka.consumer.group").Value, Is.EqualTo("sub-1"));
            Assert.That(stopped.TagObjects.First(t => t.Key == "messaging.kafka.message.key").Value, Is.EqualTo("k1"));
        }

        [Test]
        public async Task RegisterHandlerAsync_OperationCanceledFromConsume_ClosesConsumer()
        {
            var client = new TestableKafkaSubscriptionClient("localhost:1", "topic-1", "sub-1");
            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            client.SetConsumerForTest(consumerMock.Object);

            var cts = new CancellationTokenSource();
            consumerMock.Setup(c => c.Consume(It.IsAny<CancellationToken>()))
                .Returns(() => { cts.Cancel(); throw new OperationCanceledException(); });

            await client.RegisterHandlerAsync((_, _) => Task.FromResult(MessageReceivedStatus.Completed), (_, _) => Task.CompletedTask, cancellationToken: cts.Token);

            consumerMock.Verify(c => c.Unsubscribe(), Times.Once);
            consumerMock.Verify(c => c.Unassign(), Times.Once);
            consumerMock.Verify(c => c.Close(), Times.Once);
        }

        [Test]
        public async Task RegisterHandlerAsync_WithIdleHandler_InvokesItAfterSuccess()
        {
            var client = new TestableKafkaSubscriptionClient("localhost:1", "topic-1", "sub-1");
            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            client.SetConsumerForTest(consumerMock.Object);

            var cts = new CancellationTokenSource();
            consumerMock.Setup(c => c.Consume(It.IsAny<CancellationToken>()))
                .Returns(() => { cts.Cancel(); return CreateConsumeResult("k1", "payload"); });

            var idleCalled = false;
            await client.RegisterHandlerAsync((_, _) => Task.FromResult(MessageReceivedStatus.Completed), (_, _) => Task.CompletedTask
                , onIdle: _ => { idleCalled = true; return Task.CompletedTask; }, cancellationToken: cts.Token);

            Assert.That(idleCalled, Is.True);
        }

        [Test]
        public async Task RegisterHandlerAsyncGeneric_DeserializesBodyAndInvokesModelHandler()
        {
            var client = new TestableKafkaSubscriptionClient("localhost:1", "topic-1", "sub-1");
            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            client.SetConsumerForTest(consumerMock.Object);

            var cts = new CancellationTokenSource();
            consumerMock.Setup(c => c.Consume(It.IsAny<CancellationToken>()))
                .Returns(() => { cts.Cancel(); return CreateConsumeResult("k1", "{\"name\":\"abc\"}"); });

            SamplePayload? received = null;
            await client.RegisterHandlerAsync<SamplePayload>((model, ct) => { received = model; return Task.FromResult(MessageReceivedStatus.Completed); }
                , (_, _) => Task.CompletedTask, cancellationToken: cts.Token);

            Assert.That(received, Is.Not.Null);
            Assert.That(received!.Name, Is.EqualTo("abc"));
        }

        [Test]
        public async Task CloseAsync_UnsubscribesUnassignsAndCloses()
        {
            var client = new TestableKafkaSubscriptionClient("localhost:1", "topic-1", "sub-1");
            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            client.SetConsumerForTest(consumerMock.Object);

            await client.CloseAsync();

            consumerMock.Verify(c => c.Unsubscribe(), Times.Once);
            consumerMock.Verify(c => c.Unassign(), Times.Once);
            consumerMock.Verify(c => c.Close(), Times.Once);
        }

        [Test]
        public void Dispose_DisposesConsumer()
        {
            var client = new TestableKafkaSubscriptionClient("localhost:1", "topic-1", "sub-1");
            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            client.SetConsumerForTest(consumerMock.Object);

            client.Dispose();

            consumerMock.Verify(c => c.Dispose(), Times.Once);
        }

        [Test]
        public async Task DisposeAsync_DisposesConsumer()
        {
            var client = new TestableKafkaSubscriptionClient("localhost:1", "topic-1", "sub-1");
            var consumerMock = new Mock<IConsumer<string, byte[]>>();
            client.SetConsumerForTest(consumerMock.Object);

            await client.DisposeAsync();

            consumerMock.Verify(c => c.Dispose(), Times.Once);
        }
    }

    internal class SamplePayload
    {
        public string Name { get; set; } = string.Empty;
    }
}
