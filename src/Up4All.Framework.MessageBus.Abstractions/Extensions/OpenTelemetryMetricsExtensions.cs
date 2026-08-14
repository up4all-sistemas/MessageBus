using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Up4All.Framework.MessageBus.Abstractions.Extensions
{
    /// <summary>
    /// Shared helpers for the messaging metrics instrumentation. Mirrors the shape of
    /// OpenTelemetryExtensions (tracing): each broker owns its own Meter and instrument
    /// instances (created once, as static readonly fields, next to its ActivitySource -
    /// see the "Meter is a singleton per instrumentation library" guidance), but records
    /// through the extension methods here so instrument names and tag shapes stay
    /// consistent with the OpenTelemetry messaging semantic conventions across
    /// RabbitMQ/ServiceBus/Kafka.
    /// </summary>
    public static class OpenTelemetryMetricsExtensions
    {
        public const string SentMessagesInstrumentName = "messaging.client.sent.messages";
        public const string ConsumedMessagesInstrumentName = "messaging.client.consumed.messages";
        public const string OperationDurationInstrumentName = "messaging.client.operation.duration";

        public static Meter CreateMeter<TSource>()
            where TSource : class
        {
            var assemblyName = typeof(TSource).Assembly.GetName();
            return new Meter(assemblyName.Name!, assemblyName.Version?.ToString());
        }

        public static void RecordMessageSent(this Counter<long> counter, string provider, string entityPath)
        {
            counter.Add(1, BuildTags(provider, entityPath, "publish"));
        }

        public static void RecordMessageConsumed(this Counter<long> counter, string provider, string entityPath, string? errorType = null)
        {
            counter.Add(1, BuildTags(provider, entityPath, "receive", errorType));
        }

        public static void RecordOperationDuration(this Histogram<double> histogram, double elapsedSeconds, string provider, string entityPath, string operationType, string? errorType = null)
        {
            histogram.Record(elapsedSeconds, BuildTags(provider, entityPath, operationType, errorType));
        }

        private static TagList BuildTags(string provider, string entityPath, string operationType, string? errorType = null)
        {
            var tags = new TagList
            {
                { "messaging.system", provider },
                { "messaging.destination.name", entityPath },
                { "messaging.operation.type", operationType }
            };

            if (!string.IsNullOrEmpty(errorType))
                tags.Add("error.type", errorType);

            return tags;
        }
    }
}
