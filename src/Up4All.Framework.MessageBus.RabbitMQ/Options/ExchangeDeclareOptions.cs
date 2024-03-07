using System.Collections.Generic;

namespace Up4All.Framework.MessageBus.RabbitMQ.Options
{
    public class ExchangeDeclareOptions
    {
        public bool Durable { get; set; }

        public bool AutoComplete { get; set; }

        public Dictionary<string, object> Args { get; set; }

        internal ExchangeDeclareOptions()
        {
            Durable = true;            
        }
    }
}
