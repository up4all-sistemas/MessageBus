using System.Collections.Generic;

namespace Up4All.Framework.MessageBus.RabbitMQ.Options
{
    public class ExchangeDeclareOptions
    {
        public bool Durable { get; set; } = true;

        public bool AutoComplete { get; set; } = false;

        public Dictionary<string, object> Args { get; set; } = null;
    }
}
