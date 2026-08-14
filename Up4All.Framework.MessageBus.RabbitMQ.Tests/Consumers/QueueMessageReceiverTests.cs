using Moq;

using RabbitMQ.Client;

using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Consumers;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;
using Up4All.Framework.MessageBus.RabbitMQ.Tests.Support;

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests.Consumers
{
    [TestFixture]
    public class AsyncQueueMessageReceiverTests
    {
        [Test]
        public async Task HandleBasicDeliverAsync_OnSuccess_InvokesHandlerAndProcessesAsCompleted()
        {
            var channelMock = RabbitMQMocks.CreateChannel(currentQueue: "q1");
            ReceivedMessage? received = null;

            Task<MessageReceivedStatus> Handler(ReceivedMessage msg, CancellationToken ct)
            {
                received = msg;
                return Task.FromResult(MessageReceivedStatus.Completed);
            }

            var errorCalled = false;
            Task ErrorHandler(Exception ex, CancellationToken ct) { errorCalled = true; return Task.CompletedTask; }

            var receiver = new AsyncQueueMessageReceiver(channelMock.Object, Handler, ErrorHandler, null, false, RabbitMQMocks.Logger<AsyncQueueMessageReceiver>());

            var properties = new BasicProperties { MessageId = "msg-1" };
            var body = System.Text.Encoding.UTF8.GetBytes("payload");

            await receiver.HandleBasicDeliverAsync("tag", 1, false, "exchange", "rk", properties, body, CancellationToken.None);

            Assert.That(received, Is.Not.Null);
            Assert.That(received!.GetBody(), Is.EqualTo("payload"));
            Assert.That(received.UserProperties[MessageBusMessage.MessageIdkey], Is.EqualTo("msg-1"));
            Assert.That(errorCalled, Is.False);

            channelMock.Verify(c => c.BasicAckAsync(1, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task HandleBasicDeliverAsync_HandlerThrows_ProcessesAsErrorAndInvokesErrorHandler()
        {
            var channelMock = RabbitMQMocks.CreateChannel();

            Task<MessageReceivedStatus> Handler(ReceivedMessage msg, CancellationToken ct)
                => throw new InvalidOperationException("boom");

            Exception? capturedError = null;
            Task ErrorHandler(Exception ex, CancellationToken ct) { capturedError = ex; return Task.CompletedTask; }

            var receiver = new AsyncQueueMessageReceiver(channelMock.Object, Handler, ErrorHandler, null, false, RabbitMQMocks.Logger<AsyncQueueMessageReceiver>());
            var properties = new BasicProperties();
            var body = System.Text.Encoding.UTF8.GetBytes("payload");

            await receiver.HandleBasicDeliverAsync("tag", 2, false, "exchange", "rk", properties, body, CancellationToken.None);

            Assert.That(capturedError, Is.Not.Null);
            Assert.That(capturedError!.Message, Is.EqualTo("boom"));
            channelMock.Verify(c => c.BasicNackAsync(2, false, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task HandleBasicDeliverAsync_HandlerThrows_MarksActivityAsErrorAndRecordsException()
        {
            Activity? stopped = null;
            using var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
                ActivityStopped = a => stopped = a
            };
            ActivitySource.AddActivityListener(listener);

            var channelMock = RabbitMQMocks.CreateChannel();
            var exception = new InvalidOperationException("boom");
            Task<MessageReceivedStatus> Handler(ReceivedMessage msg, CancellationToken ct) => throw exception;

            var receiver = new AsyncQueueMessageReceiver(channelMock.Object, Handler, (_, _) => Task.CompletedTask, null, false, RabbitMQMocks.Logger<AsyncQueueMessageReceiver>());
            var body = System.Text.Encoding.UTF8.GetBytes("payload");

            await receiver.HandleBasicDeliverAsync("tag", 3, false, "exchange", "rk", new BasicProperties(), body, CancellationToken.None);

            Assert.That(stopped, Is.Not.Null);
            Assert.That(stopped!.Status, Is.EqualTo(ActivityStatusCode.Error));
            Assert.That(stopped.StatusDescription, Is.EqualTo("boom"));
            Assert.That(stopped.Events.Any(e => e.Name == "exception"), Is.True);
        }

        [Test]
        public async Task HandleBasicDeliverAsync_OnSuccess_RecordsConsumedCounterAndDurationWithoutErrorType()
        {
            var channelMock = RabbitMQMocks.CreateChannel(currentQueue: "q1");
            var receiver = new AsyncQueueMessageReceiver(channelMock.Object
                , (_, _) => Task.FromResult(MessageReceivedStatus.Completed)
                , (_, _) => Task.CompletedTask, null, false, RabbitMQMocks.Logger<AsyncQueueMessageReceiver>());
            var body = System.Text.Encoding.UTF8.GetBytes("payload");

            var measurements = MetricsTestHelper.CaptureMeasurements(RabbitMQClientExtensions.Meter, () =>
                receiver.HandleBasicDeliverAsync("tag", 1, false, "exchange", "rk", new BasicProperties(), body, CancellationToken.None).GetAwaiter().GetResult());

            var consumed = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.ConsumedMessagesInstrumentName);
            Assert.That(consumed.Value, Is.EqualTo(1L));
            Assert.That(consumed.Tags.First(t => t.Key == "messaging.operation.type").Value, Is.EqualTo("receive"));
            Assert.That(consumed.Tags.Any(t => t.Key == "error.type"), Is.False);

            var duration = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.OperationDurationInstrumentName);
            Assert.That(duration.Tags.Any(t => t.Key == "error.type"), Is.False);
        }

        [Test]
        public async Task HandleBasicDeliverAsync_HandlerThrows_RecordsConsumedCounterAndDurationWithErrorType()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            Task<MessageReceivedStatus> Handler(ReceivedMessage msg, CancellationToken ct) => throw new InvalidOperationException("boom");

            var receiver = new AsyncQueueMessageReceiver(channelMock.Object, Handler, (_, _) => Task.CompletedTask, null, false, RabbitMQMocks.Logger<AsyncQueueMessageReceiver>());
            var body = System.Text.Encoding.UTF8.GetBytes("payload");

            var measurements = MetricsTestHelper.CaptureMeasurements(RabbitMQClientExtensions.Meter, () =>
                receiver.HandleBasicDeliverAsync("tag", 2, false, "exchange", "rk", new BasicProperties(), body, CancellationToken.None).GetAwaiter().GetResult());

            var consumed = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.ConsumedMessagesInstrumentName);
            Assert.That(consumed.Tags.First(t => t.Key == "error.type").Value, Is.EqualTo(nameof(InvalidOperationException)));

            var duration = measurements.Single(m => m.InstrumentName == OpenTelemetryMetricsExtensions.OperationDurationInstrumentName);
            Assert.That(duration.Tags.First(t => t.Key == "error.type").Value, Is.EqualTo(nameof(InvalidOperationException)));
        }

        [Test]
        public async Task HandleBasicDeliverAsync_NoMessageId_DoesNotSetMessageId()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            ReceivedMessage? received = null;

            Task<MessageReceivedStatus> Handler(ReceivedMessage msg, CancellationToken ct)
            {
                received = msg;
                return Task.FromResult(MessageReceivedStatus.Completed);
            }

            var receiver = new AsyncQueueMessageReceiver(channelMock.Object, Handler, (_, _) => Task.CompletedTask, null, false, RabbitMQMocks.Logger<AsyncQueueMessageReceiver>());
            var properties = new BasicProperties();
            var body = System.Text.Encoding.UTF8.GetBytes("payload");

            await receiver.HandleBasicDeliverAsync("tag", 3, false, "exchange", "rk", properties, body, CancellationToken.None);

            Assert.That(received!.UserProperties.ContainsKey(MessageBusMessage.MessageIdkey), Is.False);
        }

        [Test]
        public async Task HandleBasicDeliverAsync_WithIdleHandler_InvokesItAfterSuccess()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            var idleCalled = false;

            var receiver = new AsyncQueueMessageReceiver(channelMock.Object
                , (_, _) => Task.FromResult(MessageReceivedStatus.Completed)
                , (_, _) => Task.CompletedTask
                , _ => { idleCalled = true; return Task.CompletedTask; }
                , false, RabbitMQMocks.Logger<AsyncQueueMessageReceiver>());

            var properties = new BasicProperties();
            var body = System.Text.Encoding.UTF8.GetBytes("payload");

            await receiver.HandleBasicDeliverAsync("tag", 4, false, "exchange", "rk", properties, body, CancellationToken.None);

            Assert.That(idleCalled, Is.True);
        }

        [Test]
        public async Task HandleBasicDeliverAsync_AutoComplete_DoesNotAck()
        {
            var channelMock = RabbitMQMocks.CreateChannel();

            var receiver = new AsyncQueueMessageReceiver(channelMock.Object
                , (_, _) => Task.FromResult(MessageReceivedStatus.Completed)
                , (_, _) => Task.CompletedTask, null, true, RabbitMQMocks.Logger<AsyncQueueMessageReceiver>());

            var properties = new BasicProperties();
            var body = System.Text.Encoding.UTF8.GetBytes("payload");

            await receiver.HandleBasicDeliverAsync("tag", 5, false, "exchange", "rk", properties, body, CancellationToken.None);

            channelMock.Verify(c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [TestFixture]
    public class AsyncQueueMessageReceiverForModelTests
    {
        private class Payload
        {
            public string Name { get; set; } = string.Empty;
        }

        [Test]
        public async Task HandleBasicDeliverAsync_DeserializesBodyAndInvokesModelHandler()
        {
            var channelMock = RabbitMQMocks.CreateChannel();
            Payload? received = null;

            var receiver = new AsyncQueueMessageReceiverForModel<Payload>(channelMock.Object
                , (model, ct) => { received = model; return Task.FromResult(MessageReceivedStatus.Completed); }
                , (_, _) => Task.CompletedTask, null, false, RabbitMQMocks.Logger<AsyncQueueMessageReceiverForModel<Payload>>());

            var properties = new BasicProperties();
            var body = System.Text.Encoding.UTF8.GetBytes("{\"name\":\"test\"}");

            await receiver.HandleBasicDeliverAsync("tag", 1, false, "exchange", "rk", properties, body, CancellationToken.None);

            Assert.That(received, Is.Not.Null);
            Assert.That(received!.Name, Is.EqualTo("test"));
        }
    }
}
