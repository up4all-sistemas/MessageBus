using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace Up4All.Framework.MessageBus.ServiceBus.Tests.Support
{
    internal sealed record CapturedMeasurement(string InstrumentName, object Value, IReadOnlyList<KeyValuePair<string, object?>> Tags);

    /// <summary>
    /// Captures every measurement recorded on the given Meter for the duration of
    /// `recordAction`, using System.Diagnostics.Metrics.MeterListener.
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
}
