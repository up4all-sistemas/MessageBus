namespace Up4All.Framework.MessageBus.RabbitMQ.Enums
{
    public struct Consts
    {
        public const string OpenTelemetrySourceName = "RabbitMQ Up4All MessageBus";
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
        public const string Timestamp = "mb-timestamp";
        public const string Provider = "mb-messagebus";
        public const string MessageId = "mb-id";
        public const string IsPersistent = "mb-is-persistent";
    }
}
