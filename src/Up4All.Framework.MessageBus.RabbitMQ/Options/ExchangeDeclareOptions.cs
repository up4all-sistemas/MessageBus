using System.Collections.Generic;

namespace Up4All.Framework.MessageBus.RabbitMQ.Options
{
    public class ExchangeDeclareOptions
    {
        public bool Durable { get; set; }

        public bool AutoDelete { get; set; }

        public Dictionary<string, object> Args { get; set; }

        internal ExchangeDeclareOptions()
        {
            Durable = true;
        }

        public static implicit operator ExchangeDeclareOptions(ProvisioningOptions opts)
        {
            if (opts is null) return null;

            return new ExchangeDeclareOptions()
            {
                Durable = opts.Durable,
                AutoDelete = opts.AutoDelete,
                Args = opts.Args
            };
        }
    }
}
