﻿using System.Collections.Generic;

using Up4All.Framework.MessageBus.RabbitMQ.Enums;

namespace Up4All.Framework.MessageBus.RabbitMQ.Options
{
    public class StreamDeclareOptions : QueueDeclareOptions
    {
        internal StreamDeclareOptions() : base()
        {
            Type = QueueType.Stream;
            Args ??= [];

            if (!Args.ContainsKey("x-stream-type"))
                Args.Add("x-stream-type", "stream");
        }
    }
}
