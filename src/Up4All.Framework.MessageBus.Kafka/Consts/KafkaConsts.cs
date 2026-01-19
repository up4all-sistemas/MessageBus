using System.Diagnostics;

namespace Up4All.Framework.MessageBus.Kafka.Consts
{
    public struct KafkaConsts
    {
        public const string OpenTelemetrySourceName = "Kafka Up4All MessageBus";

        public static readonly ActivitySource ActivitySource = new(OpenTelemetrySourceName);
    }
}
