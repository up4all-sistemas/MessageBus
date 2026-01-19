using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Consts
{
    public static class RabbitMQConsts
    {
        public const string OpenTelemetrySourceName = "RabbitMQ Up4All MessageBus";

        public static QueueDeclareOptions DefaultQueueDeclareOptions => new();

        public static StreamDeclareOptions DefaultStreamDeclareOptions => new();

        public static ExchangeDeclareOptions DefaultExchangeDeclareOptions => new();

        public static QueueDeclareOptions ToQueueDeclare(this ProvisioningOptions opts)
        {
            return opts;
        }

        public static StreamDeclareOptions ToStreamDeclare(this ProvisioningOptions opts)
        {
            return opts;
        }

        public static ExchangeDeclareOptions ToExchangeDeclare(this ProvisioningOptions opts)
        {
            return opts;
        }

    }

    public struct ExchangeType
    {
        public const string Direct = "direct";
        public const string Fanout = "fanout";
        public const string Headers = "headers";
        public const string Topic = "topic";
    }

    public struct QueueType
    {
        public const string Classic = "classic";
        public const string Quorum = "quorum";
        public const string Stream = "stream";
    }

    public struct OffsetType
    {
        public const string First = "first";
        public const string Last = "last";
        public const string Next = "next";
    }

    public struct Arguments
    {
        public const string StreamOffsetKey = "x-stream-offset";
    }

    public struct Properties
    {        
        public const string IsPersistent = "mb-is-persistent";
    }
}
