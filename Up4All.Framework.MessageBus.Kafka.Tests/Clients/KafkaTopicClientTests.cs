using Confluent.Kafka;

using Moq;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Kafka.Extensions;
using Up4All.Framework.MessageBus.Kafka.Tests.Support;

namespace Up4All.Framework.MessageBus.Kafka.Tests.Clients
{
    [TestFixture]
    public class KafkaStandaloneGenericTopicAsyncClientTests
    {
        private static (KafkaStandaloneGenericTopicAsyncClient<string> client, Mock<IProducer<string, byte[]>> producer) CreateClient()
        {
            var client = new KafkaStandaloneGenericTopicAsyncClient<string>("localhost:1", "topic-1", connectionAttempts: 1);
            var producerMock = new Mock<IProducer<string, byte[]>>();
            producerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, byte[]>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeliveryResult<string, byte[]>());

            KafkaTestHelpers.ReplaceProducer(client, producerMock.Object);
            return (client, producerMock);
        }

        [Test]
        public void Constructor_ExposesTopicName()
        {
            var (client, _) = CreateClient();

            Assert.That(client.TopicName, Is.EqualTo("topic-1"));
        }

        [Test]
        public async Task SendAsync_ProducesMessageToTopic()
        {
            var (client, producer) = CreateClient();
            var message = new MessageBusMessage();
            message.AddBody("payload");
            message.SetMessageId("key-1");

            await client.SendAsync(message);

            producer.Verify(p => p.ProduceAsync("topic-1", It.Is<Message<string, byte[]>>(m => m.Key == "key-1"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendAsync_Enumerable_ProducesEachMessage()
        {
            var (client, producer) = CreateClient();
            var messages = new[] { new MessageBusMessage(), new MessageBusMessage() };
            foreach (var m in messages) { m.AddBody("x"); m.SetMessageId("k"); }

            await client.SendAsync((System.Collections.Generic.IEnumerable<MessageBusMessage>)messages);

            producer.Verify(p => p.ProduceAsync("topic-1", It.IsAny<Message<string, byte[]>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public async Task SendAsync_Model_SerializesAndProduces()
        {
            var (client, producer) = CreateClient();

            await client.SendAsync(new SamplePayload { Name = "abc" });

            producer.Verify(p => p.ProduceAsync("topic-1", It.IsAny<Message<string, byte[]>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SendManyAsync_ProducesEachModel()
        {
            var (client, producer) = CreateClient();

            await client.SendManyAsync(new[] { new SamplePayload { Name = "a" }, new SamplePayload { Name = "b" } });

            producer.Verify(p => p.ProduceAsync("topic-1", It.IsAny<Message<string, byte[]>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public async Task SendAsync_Success_RecordsSentCounterAndDurationWithoutErrorType()
        {
            var (client, _) = CreateClient();
            var message = new MessageBusMessage();
            message.AddBody("payload");
            message.SetMessageId("key-1");

            var measurements = MetricsTestHelper.CaptureMeasurements(KafkaExtensions.Meter, () =>
                client.SendAsync(message).GetAwaiter().GetResult());

            var sent = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.SentMessagesInstrumentName);
            Assert.That(sent.Value, Is.EqualTo(1L));
            Assert.That(sent.Tags.Any(t => t.Key == "error.type"), Is.False);

            var duration = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.OperationDurationInstrumentName);
            Assert.That(duration.Tags.First(t => t.Key == "messaging.operation.type").Value, Is.EqualTo("publish"));
            Assert.That(duration.Tags.Any(t => t.Key == "error.type"), Is.False);
        }

        [Test]
        public void SendAsync_ProduceThrows_RecordsDurationWithErrorTypeAndDoesNotRecordSentCounter()
        {
            var client = new KafkaStandaloneGenericTopicAsyncClient<string>("localhost:1", "topic-1", connectionAttempts: 1);
            var producerMock = new Mock<IProducer<string, byte[]>>();
            producerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, byte[]>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));
            KafkaTestHelpers.ReplaceProducer(client, producerMock.Object);

            var message = new MessageBusMessage();
            message.AddBody("payload");
            message.SetMessageId("key-1");

            var measurements = new System.Collections.Generic.List<CapturedMeasurement>();
            using var listener = new System.Diagnostics.Metrics.MeterListener();
            listener.InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter == KafkaExtensions.Meter)
                    l.EnableMeasurementEvents(instrument);
            };
            listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
                measurements.Add(new CapturedMeasurement(instrument.Name, value, tags.ToArray())));
            listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
                measurements.Add(new CapturedMeasurement(instrument.Name, value, tags.ToArray())));
            listener.Start();

            Assert.ThrowsAsync<InvalidOperationException>(() => client.SendAsync(message));

            Assert.That(measurements.Any(m => m.InstrumentName == OpenTelemetryMetricsExtensions.SentMessagesInstrumentName), Is.False);

            var duration = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.OperationDurationInstrumentName);
            Assert.That(duration.Tags.First(t => t.Key == "error.type").Value, Is.EqualTo(nameof(InvalidOperationException)));
        }

        [Test]
        public void Dispose_DisposesProducer()
        {
            var (client, producer) = CreateClient();

            client.Dispose();

            producer.Verify(p => p.Dispose(), Times.Once);
        }

        [Test]
        public async Task DisposeAsync_DisposesProducer()
        {
            var (client, producer) = CreateClient();

            await client.DisposeAsync();

            producer.Verify(p => p.Dispose(), Times.Once);
        }
    }

    [TestFixture]
    public class KafkaStandaloneTopicAsyncClientTests
    {
        [Test]
        public async Task SendAsync_AssignsRandomMessageIdWhenNotSet()
        {
            var client = new KafkaStandaloneTopicAsyncClient("localhost:1", "topic-1", connectionAttempts: 1);
            var producerMock = new Mock<IProducer<string, byte[]>>();
            producerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, byte[]>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeliveryResult<string, byte[]>());
            KafkaTestHelpers.ReplaceProducer(client, producerMock.Object);

            var message = new MessageBusMessage();
            message.AddBody("payload");

            await client.SendAsync(message);

            producerMock.Verify(p => p.ProduceAsync("topic-1"
                , It.Is<Message<string, byte[]>>(m => !string.IsNullOrEmpty(m.Key)), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
