using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

using Up4All.Framework.MessageBus.Abstractions.Extensions;

namespace Up4All.Framework.MessageBus.Tests.Extensions
{
    [TestFixture]
    public class OpenTelemetryMetricsExtensionsTests
    {
        private sealed record Measurement(string InstrumentName, object Value, IReadOnlyList<KeyValuePair<string, object?>> Tags);

        private static List<Measurement> CaptureMeasurements(Meter meter, System.Action recordAction)
        {
            var measurements = new List<Measurement>();

            using var listener = new MeterListener();
            listener.InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter == meter)
                    l.EnableMeasurementEvents(instrument);
            };
            listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
                measurements.Add(new Measurement(instrument.Name, value, tags.ToArray())));
            listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
                measurements.Add(new Measurement(instrument.Name, value, tags.ToArray())));
            listener.Start();

            recordAction();

            return measurements;
        }

        [Test]
        public void RecordMessageSent_RecordsSingleIncrementWithPublishOperationType()
        {
            using var meter = new Meter(nameof(RecordMessageSent_RecordsSingleIncrementWithPublishOperationType));
            var counter = meter.CreateCounter<long>("test.sent");

            var measurements = CaptureMeasurements(meter, () => counter.RecordMessageSent("rabbitmq", "queue-1"));

            var measurement = measurements.Single();
            Assert.That(measurement.Value, Is.EqualTo(1L));
            Assert.That(measurement.Tags.First(t => t.Key == "messaging.system").Value, Is.EqualTo("rabbitmq"));
            Assert.That(measurement.Tags.First(t => t.Key == "messaging.destination.name").Value, Is.EqualTo("queue-1"));
            Assert.That(measurement.Tags.First(t => t.Key == "messaging.operation.type").Value, Is.EqualTo("publish"));
            Assert.That(measurement.Tags.Any(t => t.Key == "error.type"), Is.False);
        }

        [Test]
        public void RecordMessageConsumed_WithoutError_DoesNotIncludeErrorTypeTag()
        {
            using var meter = new Meter(nameof(RecordMessageConsumed_WithoutError_DoesNotIncludeErrorTypeTag));
            var counter = meter.CreateCounter<long>("test.consumed");

            var measurements = CaptureMeasurements(meter, () => counter.RecordMessageConsumed("rabbitmq", "queue-1"));

            var measurement = measurements.Single();
            Assert.That(measurement.Tags.First(t => t.Key == "messaging.operation.type").Value, Is.EqualTo("receive"));
            Assert.That(measurement.Tags.Any(t => t.Key == "error.type"), Is.False);
        }

        [Test]
        public void RecordMessageConsumed_WithError_IncludesErrorTypeTag()
        {
            using var meter = new Meter(nameof(RecordMessageConsumed_WithError_IncludesErrorTypeTag));
            var counter = meter.CreateCounter<long>("test.consumed");

            var measurements = CaptureMeasurements(meter, () => counter.RecordMessageConsumed("rabbitmq", "queue-1", "InvalidOperationException"));

            var measurement = measurements.Single();
            Assert.That(measurement.Tags.First(t => t.Key == "error.type").Value, Is.EqualTo("InvalidOperationException"));
        }

        [Test]
        public void RecordOperationDuration_RecordsElapsedSecondsWithGivenOperationType()
        {
            using var meter = new Meter(nameof(RecordOperationDuration_RecordsElapsedSecondsWithGivenOperationType));
            var histogram = meter.CreateHistogram<double>("test.duration");

            var measurements = CaptureMeasurements(meter, () => histogram.RecordOperationDuration(1.5, "rabbitmq", "queue-1", "publish"));

            var measurement = measurements.Single();
            Assert.That(measurement.Value, Is.EqualTo(1.5));
            Assert.That(measurement.Tags.First(t => t.Key == "messaging.operation.type").Value, Is.EqualTo("publish"));
        }
    }
}
