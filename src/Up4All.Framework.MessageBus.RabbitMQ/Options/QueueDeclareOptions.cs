using System;
using System.Collections.Generic;

namespace Up4All.Framework.MessageBus.RabbitMQ.Options
{
    public class QueueDeclareOptions
    {
        public bool Exclusive { get; set; } = false;

        public bool Durable { get; set; } = true;

        public bool AutoComplete { get; set; } = false;

        public Dictionary<string, object> Args { get; set; } = null;

        public ICollection<QueueBindOptions> Bindings { get; set; } = new List<QueueBindOptions>();

        public QueueDeclareOptions()
        {
        }

        public QueueDeclareOptions(Action<QueueBindOptions> createBinding) : this()
        {
            var binding = new QueueBindOptions();
            createBinding(binding);
            Bindings.Add(binding);
        }
    }

    public class QueueBindOptions
    {
        public string ExchangeName { get; set; }

        public string RoutingKey { get; set; }

        public Dictionary<string, object> Args { get; set; }

        internal QueueBindOptions()
        {
            Args = new Dictionary<string, object>();
        }

        public QueueBindOptions(string exchangeName, string defaultArgKey, object defaultArgValue, string routingkey = null) : this()
        {
            ExchangeName = exchangeName;
            RoutingKey = routingkey;
            Args.Add(defaultArgKey, defaultArgValue);
        }
    }
}
