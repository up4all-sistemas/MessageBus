using System;
using System.Collections.Generic;
using System.Linq;

using Up4All.Framework.MessageBus.RabbitMQ.Consts;

namespace Up4All.Framework.MessageBus.RabbitMQ.Options
{
    public class QueueDeclareOptions
    {
        public bool Exclusive { get; set; }

        public bool Durable { get; set; }

        public bool AutoDelete { get; set; }

        public Dictionary<string, object> Args { get; set; }

        public ICollection<QueueBindOptions> Bindings { get; set; }

        public string Type { get; set; }

        internal QueueDeclareOptions()
        {
            Bindings = [];
            Args = [];
            Type = QueueType.Classic;
            Durable = true;
        }

        public void AddBinding(string exchangeName, Action<QueueBindOptions> createBinding)
        {
            var binding = new QueueBindOptions(exchangeName);
            createBinding(binding);
            Bindings.Add(binding);
        }

        public static implicit operator QueueDeclareOptions(ProvisioningOptions opts)
        {
            if (opts is null) return null;

            return new QueueDeclareOptions
            {
                Exclusive = opts.Exclusive,
                Durable = opts.Durable,
                AutoDelete = opts.AutoDelete,
                Args = opts.Args,
                Bindings = [.. opts.Bindings.Select(x => (QueueBindOptions)x)]
            };
        }

    }

    public class QueueBindOptions
    {
        public string ExchangeName { get; set; }

        public string RoutingKey { get; set; }

        public Dictionary<string, object> Args { get; set; }

        internal QueueBindOptions(string exchangeName)
        {
            ExchangeName = exchangeName;
            Args = [];
        }

        public QueueBindOptions(string exchangeName, string defaultArgKey, object defaultArgValue, string routingkey = null) : this(exchangeName)
        {
            RoutingKey = routingkey;
            Args.Add(defaultArgKey, defaultArgValue);
        }

        public static implicit operator QueueBindOptions(ProvisioningBindingOptions opts)
        {
            return new QueueBindOptions(opts.ExchangeName)
            {
                RoutingKey = opts.RoutingKey,
                Args = opts.Args
            };
        }
    }
}
