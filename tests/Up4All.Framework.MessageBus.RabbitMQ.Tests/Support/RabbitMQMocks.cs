using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests.Support
{
    internal sealed record CapturedMeasurement(string InstrumentName, object Value, IReadOnlyList<KeyValuePair<string, object?>> Tags);

    /// <summary>
    /// Captures every measurement recorded on the given Meter for the duration of
    /// `recordAction`, using System.Diagnostics.Metrics.MeterListener - the same mechanism
    /// GlobalTestSetup could use for a listener wired for the whole assembly, but scoped
    /// per-test here since we only care about a handful of specific instruments.
    /// </summary>
    internal static class MetricsTestHelper
    {
        public static List<CapturedMeasurement> CaptureMeasurements(Meter meter, Action recordAction)
        {
            var measurements = new List<CapturedMeasurement>();

            using var listener = new MeterListener();
            listener.InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter == meter)
                    l.EnableMeasurementEvents(instrument);
            };
            listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
                measurements.Add(new CapturedMeasurement(instrument.Name, value, tags.ToArray())));
            listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
                measurements.Add(new CapturedMeasurement(instrument.Name, value, tags.ToArray())));
            listener.Start();

            recordAction();

            return measurements;
        }
    }

    /// <summary>
    /// Helpers to build loosely-configured Moq doubles for the RabbitMQ.Client interfaces,
    /// with the commonly awaited members pre-wired to complete successfully so tests only
    /// need to override/verify what they actually care about.
    /// </summary>
    internal static class RabbitMQMocks
    {
        public static Mock<IChannel> CreateChannel(bool isOpen = true, string currentQueue = "")
        {
            var mock = new Mock<IChannel>();

            mock.SetupGet(c => c.IsOpen).Returns(isOpen);
            mock.SetupGet(c => c.CurrentQueue).Returns(currentQueue);

            mock.Setup(c => c.BasicQosAsync(It.IsAny<uint>(), It.IsAny<ushort>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mock.Setup(c => c.BasicConsumeAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()
                    , It.IsAny<IDictionary<string, object?>>(), It.IsAny<IAsyncBasicConsumer>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("consumer-tag");

            mock.Setup(c => c.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BasicProperties>()
                    , It.IsAny<System.ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
                .Returns(default(ValueTask));

            mock.Setup(c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(default(ValueTask));

            mock.Setup(c => c.BasicNackAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(default(ValueTask));

            mock.Setup(c => c.BasicRejectAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(default(ValueTask));

            mock.Setup(c => c.QueueDeclareAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()
                    , It.IsAny<IDictionary<string, object?>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QueueDeclareOk("queue", 0, 0));

            mock.Setup(c => c.QueueBindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()
                    , It.IsAny<IDictionary<string, object?>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mock.Setup(c => c.ExchangeDeclareAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()
                    , It.IsAny<IDictionary<string, object?>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mock.Setup(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return mock;
        }

        public static Mock<IConnection> CreateConnection(IChannel channel)
        {
            var mock = new Mock<IConnection>();

            mock.SetupGet(c => c.IsOpen).Returns(true);

            mock.Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(channel);

            mock.Setup(c => c.CloseAsync(It.IsAny<ushort>(), It.IsAny<string>(), It.IsAny<System.TimeSpan>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return mock;
        }

        public static NullLogger<T> Logger<T>() => NullLogger<T>.Instance;
    }
}
