using System;
using System.Collections.Generic;
using System.Text;

namespace Up4All.Framework.MessageBus.RabbitMQ.Enums
{
    public struct ExchangeType
    {
        public const string Direct = "direct";
        public const string Fanout = "fanout";
        public const string Headers = "headers";
        public const string Topic = "topic";
    }

    public struct QueueType
    {
        public const string Default = "default";
        public const string Classic = "classic";
        public const string Quorum = "quorum";
        public const string Stream = "stream";
    }
}
