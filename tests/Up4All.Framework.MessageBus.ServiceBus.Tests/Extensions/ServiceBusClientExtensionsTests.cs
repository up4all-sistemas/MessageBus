using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.ServiceBus.Extensions;
using Up4All.Framework.MessageBus.ServiceBus.Tests.Support;

namespace Up4All.Framework.MessageBus.ServiceBus.Tests.Extensions
{
    [TestFixture]
    public class ServiceBusClientExtensionsTests
    {
        [Test]
        public void Meter_IsASingletonAcrossCalls()
        {
            var first = ServiceBusClientExtensions.Meter;
            var second = ServiceBusClientExtensions.Meter;

            Assert.That(first, Is.SameAs(second));
        }

        [Test]
        public void PrepareMesssage_SetsBodyContentTypeAndHeaders()
        {
            var message = new MessageBusMessage();
            message.AddBody(BinaryData.FromString("{\"a\":1}"), isJsonData: true);
            message.AddUserProperty("k", "v");

            var sbMessage = ServiceBusClientExtensions.PrepareMesssage(message);

            Assert.That(Encoding.UTF8.GetString(sbMessage.Body), Is.EqualTo("{\"a\":1}"));
            Assert.That(sbMessage.ContentType, Is.EqualTo("application/json"));
            Assert.That(sbMessage.ApplicationProperties["k"], Is.EqualTo("v"));
        }

        [Test]
        public void PrepareMesssage_NonJsonBody_DoesNotSetContentType()
        {
            var message = new MessageBusMessage();
            message.AddBody("plain");

            var sbMessage = ServiceBusClientExtensions.PrepareMesssage(message);

            Assert.That(sbMessage.ContentType, Is.Null);
        }

        [Test]
        public void PrepareMesssage_WithCorrelationId_SetsIt()
        {
            var message = new MessageBusMessage();
            message.AddBody("payload");
            var correlationId = Guid.NewGuid();
            message.SetCorrelationId(correlationId);

            var sbMessage = ServiceBusClientExtensions.PrepareMesssage(message);

            Assert.That(sbMessage.CorrelationId, Is.EqualTo(correlationId.ToString()));
        }

        [Test]
        public void PrepareMesssage_AddsTraceProperties()
        {
            var message = new MessageBusMessage();
            message.AddBody("payload");

            var sbMessage = ServiceBusClientExtensions.PrepareMesssage(message);

            Assert.That(sbMessage.ApplicationProperties.ContainsKey("mb-messagebus"), Is.True);
            Assert.That(sbMessage.ApplicationProperties["mb-messagebus"], Is.EqualTo("servicebus"));
        }

        [Test]
        public void PrepareMesssage_Generic_SerializesModel()
        {
            var model = new SamplePayload { Name = "abc" };

            var sbMessage = ServiceBusClientExtensions.PrepareMesssage(model);

            Assert.That(Encoding.UTF8.GetString(sbMessage.Body), Does.Contain("abc"));
        }

        [Test]
        public void CreateClient_WithEntityName_ReturnsClientAndSender()
        {
            var (client, sender) = ServiceBusClientExtensions.CreateClient(NullLogger.Instance
                , "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ZmFrZWtleQ==", "queue-1", 1);

            Assert.That(client, Is.Not.Null);
            Assert.That(sender.EntityPath, Is.EqualTo("queue-1"));
        }

        [Test]
        public void CreateClient_WithoutEntityName_ReturnsClient()
        {
            var client = ServiceBusClientExtensions.CreateClient(
                "Endpoint=sb://fake.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ZmFrZWtleQ==", 1);

            Assert.That(client, Is.Not.Null);
        }

        [Test]
        public async Task SendMessageBusMessageAsync_Single_PublishesPreparedMessage()
        {
            var senderMock = new Mock<ServiceBusSender>();
            senderMock.SetupGet(s => s.EntityPath).Returns("my-entity");
            ServiceBusMessage? captured = null;
            senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
                .Callback<ServiceBusMessage, CancellationToken>((m, _) => captured = m)
                .Returns(Task.CompletedTask);

            var message = new MessageBusMessage();
            message.AddBody("payload");

            await senderMock.Object.SendMessageBusMessageAsync(NullLogger.Instance, message, CancellationToken.None);

            Assert.That(captured, Is.Not.Null);
            Assert.That(Encoding.UTF8.GetString(captured!.Body), Is.EqualTo("payload"));
        }

        [Test]
        public async Task SendMessageBusMessageAsync_TagsActivityWithPublishOperationType()
        {
            // "publish" is the value defined by the OpenTelemetry messaging semantic
            // conventions for a producer-side operation; this used to fall back to the
            // "receive" default because no operationType was passed at all.
            System.Diagnostics.Activity? stopped = null;
            using var listener = new System.Diagnostics.ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref System.Diagnostics.ActivityCreationOptions<System.Diagnostics.ActivityContext> options) => System.Diagnostics.ActivitySamplingResult.AllData,
                ActivityStopped = a => stopped = a
            };
            System.Diagnostics.ActivitySource.AddActivityListener(listener);

            var senderMock = new Mock<ServiceBusSender>();
            senderMock.SetupGet(s => s.EntityPath).Returns("my-entity");
            senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var message = new MessageBusMessage();
            message.AddBody("payload");

            await senderMock.Object.SendMessageBusMessageAsync(NullLogger.Instance, message, CancellationToken.None);

            Assert.That(stopped, Is.Not.Null);
            Assert.That(stopped!.GetTagItem("messaging.operation.type"), Is.EqualTo("publish"));
        }

        [Test]
        public async Task SendMessageBusMessageAsync_Success_RecordsSentCounterAndDurationWithoutErrorType()
        {
            var senderMock = new Mock<ServiceBusSender>();
            senderMock.SetupGet(s => s.EntityPath).Returns("my-entity");
            senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var message = new MessageBusMessage();
            message.AddBody("payload");

            var measurements = MetricsTestHelper.CaptureMeasurements(ServiceBusClientExtensions.Meter, () =>
                senderMock.Object.SendMessageBusMessageAsync(NullLogger.Instance, message, CancellationToken.None).GetAwaiter().GetResult());

            var sent = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.SentMessagesInstrumentName);
            Assert.That(sent.Value, Is.EqualTo(1L));
            Assert.That(sent.Tags.Any(t => t.Key == "error.type"), Is.False);

            var duration = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.OperationDurationInstrumentName);
            Assert.That(duration.Tags.First(t => t.Key == "messaging.operation.type").Value, Is.EqualTo("publish"));
            Assert.That(duration.Tags.Any(t => t.Key == "error.type"), Is.False);
        }

        [Test]
        public void SendMessageBusMessageAsync_SendThrows_RecordsDurationWithErrorTypeAndDoesNotRecordSentCounter()
        {
            var senderMock = new Mock<ServiceBusSender>();
            senderMock.SetupGet(s => s.EntityPath).Returns("my-entity");
            senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var message = new MessageBusMessage();
            message.AddBody("payload");

            var measurements = new List<CapturedMeasurement>();
            using var listener = new System.Diagnostics.Metrics.MeterListener();
            listener.InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter == ServiceBusClientExtensions.Meter)
                    l.EnableMeasurementEvents(instrument);
            };
            listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
                measurements.Add(new CapturedMeasurement(instrument.Name, value, tags.ToArray())));
            listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
                measurements.Add(new CapturedMeasurement(instrument.Name, value, tags.ToArray())));
            listener.Start();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                senderMock.Object.SendMessageBusMessageAsync(NullLogger.Instance, message, CancellationToken.None));

            Assert.That(measurements.Any(m => m.InstrumentName == OpenTelemetryMetricsExtensions.SentMessagesInstrumentName), Is.False);

            var duration = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.OperationDurationInstrumentName);
            Assert.That(duration.Tags.First(t => t.Key == "error.type").Value, Is.EqualTo(nameof(InvalidOperationException)));
        }

        [Test]
        public async Task SendMessageBusMessageAsync_Enumerable_PublishesEachMessage()
        {
            var senderMock = new Mock<ServiceBusSender>();
            senderMock.SetupGet(s => s.EntityPath).Returns("my-entity");
            senderMock.Setup(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var messages = new[] { new MessageBusMessage(), new MessageBusMessage() };
            foreach (var m in messages) m.AddBody("x");

            await senderMock.Object.SendMessageBusMessageAsync(NullLogger.Instance, messages, CancellationToken.None);

            senderMock.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public async Task RegisterHandleMessageAsync_SuccessfulMessage_InvokesHandlerAndCompletes()
        {
            var processor = new TestableServiceBusProcessor();
            ReceivedMessage? received = null;

            Task<MessageReceivedStatus> Handler(ReceivedMessage msg, CancellationToken ct)
            {
                received = msg;
                return Task.FromResult(MessageReceivedStatus.Completed);
            }

            var errorCalled = false;
            Task ErrorHandler(Exception ex, CancellationToken ct) { errorCalled = true; return Task.CompletedTask; }

            await processor.RegisterHandleMessageAsync(NullLogger.Instance, Handler, ErrorHandler, autoComplete: false, cancellationToken: CancellationToken.None);

            var sbMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: BinaryData.FromString("payload"), messageId: "msg-1");
            var receiverMock = new Mock<ServiceBusReceiver>();
            receiverMock.Setup(r => r.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            var args = new ProcessMessageEventArgs(sbMessage, receiverMock.Object, CancellationToken.None);

            await processor.RaiseProcessMessageAsync(args);

            Assert.That(received, Is.Not.Null);
            Assert.That(received!.GetBody(), Is.EqualTo("payload"));
            Assert.That(errorCalled, Is.False);
            receiverMock.Verify(r => r.CompleteMessageAsync(sbMessage, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task RegisterHandleMessageAsync_AutoComplete_DoesNotCompleteManually()
        {
            var processor = new TestableServiceBusProcessor();

            await processor.RegisterHandleMessageAsync(NullLogger.Instance
                , (_, _) => Task.FromResult(MessageReceivedStatus.Completed), (_, _) => Task.CompletedTask
                , autoComplete: true, cancellationToken: CancellationToken.None);

            var sbMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString("x"), messageId: "id");
            var receiverMock = new Mock<ServiceBusReceiver>();
            var args = new ProcessMessageEventArgs(sbMessage, receiverMock.Object, CancellationToken.None);

            await processor.RaiseProcessMessageAsync(args);

            receiverMock.Verify(r => r.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task RegisterHandleMessageAsync_DeadletterResult_DeadLettersMessage()
        {
            var processor = new TestableServiceBusProcessor();

            await processor.RegisterHandleMessageAsync(NullLogger.Instance
                , (_, _) => Task.FromResult(MessageReceivedStatus.Deadletter), (_, _) => Task.CompletedTask
                , autoComplete: false, cancellationToken: CancellationToken.None);

            var sbMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString("x"), messageId: "id");
            var receiverMock = new Mock<ServiceBusReceiver>();
            receiverMock.Setup(r => r.DeadLetterMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            receiverMock.Setup(r => r.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            var args = new ProcessMessageEventArgs(sbMessage, receiverMock.Object, CancellationToken.None);

            await processor.RaiseProcessMessageAsync(args);

            receiverMock.Verify(r => r.DeadLetterMessageAsync(sbMessage, It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task RegisterHandleMessageAsync_AbandonedResult_AbandonsMessage()
        {
            var processor = new TestableServiceBusProcessor();

            await processor.RegisterHandleMessageAsync(NullLogger.Instance
                , (_, _) => Task.FromResult(MessageReceivedStatus.Abandoned), (_, _) => Task.CompletedTask
                , autoComplete: false, cancellationToken: CancellationToken.None);

            var sbMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString("x"), messageId: "id");
            var receiverMock = new Mock<ServiceBusReceiver>();
            receiverMock.Setup(r => r.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            receiverMock.Setup(r => r.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            var args = new ProcessMessageEventArgs(sbMessage, receiverMock.Object, CancellationToken.None);

            await processor.RaiseProcessMessageAsync(args);

            receiverMock.Verify(r => r.AbandonMessageAsync(sbMessage, It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task RegisterHandleMessageAsync_HandlerThrows_AbandonsAndInvokesErrorHandler()
        {
            var processor = new TestableServiceBusProcessor();
            Exception? capturedError = null;

            Task<MessageReceivedStatus> Handler(ReceivedMessage msg, CancellationToken ct) => throw new InvalidOperationException("boom");
            Task ErrorHandler(Exception ex, CancellationToken ct) { capturedError = ex; return Task.CompletedTask; }

            await processor.RegisterHandleMessageAsync(NullLogger.Instance, Handler, ErrorHandler, autoComplete: false, cancellationToken: CancellationToken.None);

            var sbMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString("x"), messageId: "id");
            var receiverMock = new Mock<ServiceBusReceiver>();
            receiverMock.Setup(r => r.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            var args = new ProcessMessageEventArgs(sbMessage, receiverMock.Object, CancellationToken.None);

            await processor.RaiseProcessMessageAsync(args);

            Assert.That(capturedError, Is.Not.Null);
            Assert.That(capturedError!.Message, Is.EqualTo("boom"));
            receiverMock.Verify(r => r.AbandonMessageAsync(sbMessage, It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task RegisterHandleMessageAsync_HandlerThrows_MarksActivityAsErrorAndRecordsException()
        {
            System.Diagnostics.Activity? stopped = null;
            using var listener = new System.Diagnostics.ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref System.Diagnostics.ActivityCreationOptions<System.Diagnostics.ActivityContext> options) => System.Diagnostics.ActivitySamplingResult.AllData,
                ActivityStopped = a => stopped = a
            };
            System.Diagnostics.ActivitySource.AddActivityListener(listener);

            var processor = new TestableServiceBusProcessor();
            var exception = new InvalidOperationException("boom");
            await processor.RegisterHandleMessageAsync(NullLogger.Instance, (_, _) => throw exception, (_, _) => Task.CompletedTask
                , autoComplete: false, cancellationToken: CancellationToken.None);

            var sbMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString("x"), messageId: "id");
            var receiverMock = new Mock<ServiceBusReceiver>();
            receiverMock.Setup(r => r.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            var args = new ProcessMessageEventArgs(sbMessage, receiverMock.Object, CancellationToken.None);

            await processor.RaiseProcessMessageAsync(args);

            Assert.That(stopped, Is.Not.Null);
            Assert.That(stopped!.Status, Is.EqualTo(System.Diagnostics.ActivityStatusCode.Error));
            Assert.That(stopped.StatusDescription, Is.EqualTo("boom"));
            Assert.That(stopped.Events.Any(e => e.Name == "exception"), Is.True);
        }

        [Test]
        public async Task RegisterHandleMessageAsync_OnSuccess_RecordsConsumedCounterAndDurationWithoutErrorType()
        {
            var processor = new TestableServiceBusProcessor();

            await processor.RegisterHandleMessageAsync(NullLogger.Instance
                , (_, _) => Task.FromResult(MessageReceivedStatus.Completed), (_, _) => Task.CompletedTask
                , autoComplete: false, cancellationToken: CancellationToken.None);

            var sbMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString("x"), messageId: "id");
            var receiverMock = new Mock<ServiceBusReceiver>();
            receiverMock.Setup(r => r.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var args = new ProcessMessageEventArgs(sbMessage, receiverMock.Object, CancellationToken.None);

            var measurements = MetricsTestHelper.CaptureMeasurements(ServiceBusClientExtensions.Meter, () =>
                processor.RaiseProcessMessageAsync(args).GetAwaiter().GetResult());

            var consumed = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.ConsumedMessagesInstrumentName);
            Assert.That(consumed.Value, Is.EqualTo(1L));
            Assert.That(consumed.Tags.First(t => t.Key == "messaging.operation.type").Value, Is.EqualTo("receive"));
            Assert.That(consumed.Tags.Any(t => t.Key == "error.type"), Is.False);

            var duration = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.OperationDurationInstrumentName);
            Assert.That(duration.Tags.Any(t => t.Key == "error.type"), Is.False);
        }

        [Test]
        public async Task RegisterHandleMessageAsync_HandlerThrows_RecordsConsumedCounterAndDurationWithErrorType()
        {
            var processor = new TestableServiceBusProcessor();
            var exception = new InvalidOperationException("boom");

            await processor.RegisterHandleMessageAsync(NullLogger.Instance, (_, _) => throw exception, (_, _) => Task.CompletedTask
                , autoComplete: false, cancellationToken: CancellationToken.None);

            var sbMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString("x"), messageId: "id");
            var receiverMock = new Mock<ServiceBusReceiver>();
            receiverMock.Setup(r => r.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            var args = new ProcessMessageEventArgs(sbMessage, receiverMock.Object, CancellationToken.None);

            var measurements = MetricsTestHelper.CaptureMeasurements(ServiceBusClientExtensions.Meter, () =>
                processor.RaiseProcessMessageAsync(args).GetAwaiter().GetResult());

            var consumed = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.ConsumedMessagesInstrumentName);
            Assert.That(consumed.Tags.First(t => t.Key == "error.type").Value, Is.EqualTo(nameof(InvalidOperationException)));

            var duration = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.OperationDurationInstrumentName);
            Assert.That(duration.Tags.First(t => t.Key == "error.type").Value, Is.EqualTo(nameof(InvalidOperationException)));
        }

        [Test]
        public async Task RegisterHandleMessageAsync_WithIdleHandler_InvokesItAfterSuccess()
        {
            var processor = new TestableServiceBusProcessor();
            var idleCalled = false;

            await processor.RegisterHandleMessageAsync(NullLogger.Instance
                , (_, _) => Task.FromResult(MessageReceivedStatus.Completed), (_, _) => Task.CompletedTask
                , onIdle: _ => { idleCalled = true; return Task.CompletedTask; }
                , autoComplete: false, cancellationToken: CancellationToken.None);

            var sbMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString("x"), messageId: "id");
            var receiverMock = new Mock<ServiceBusReceiver>();
            receiverMock.Setup(r => r.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var args = new ProcessMessageEventArgs(sbMessage, receiverMock.Object, CancellationToken.None);

            await processor.RaiseProcessMessageAsync(args);

            Assert.That(idleCalled, Is.True);
        }

        [Test]
        public async Task RegisterHandleMessageAsync_ProcessErrorAsync_InvokesErrorHandler()
        {
            var processor = new TestableServiceBusProcessor();
            Exception? captured = null;

            await processor.RegisterHandleMessageAsync(NullLogger.Instance
                , (_, _) => Task.FromResult(MessageReceivedStatus.Completed)
                , (ex, _) => { captured = ex; return Task.CompletedTask; }
                , autoComplete: false, cancellationToken: CancellationToken.None);

            var errorArgs = new ProcessErrorEventArgs(new InvalidOperationException("broker error"), ServiceBusErrorSource.Receive
                , "fake.servicebus.windows.net", "entity", CancellationToken.None);

            await processor.RaiseProcessErrorAsync(errorArgs);

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.Message, Is.EqualTo("broker error"));
        }
    }

    internal class SamplePayload
    {
        public string Name { get; set; } = string.Empty;
    }
}
