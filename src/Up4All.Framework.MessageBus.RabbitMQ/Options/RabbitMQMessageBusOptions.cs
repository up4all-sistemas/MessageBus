using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Enums;

namespace Up4All.Framework.MessageBus.RabbitMQ.Options
{
    public class RabbitMQMessageBusOptions : MessageBusOptions
    {
        public bool PersistentMessages { get; set; } = true;

        public ProvisioningOptions ProvisioningOptions { get; set; } = null;

        public bool ProvisioningProvided => ProvisioningOptions is not null;
    }

    public class ProvisioningOptions
    {
        public string Type { get; set; } = QueueType.Classic;

        public bool Exclusive { get; set; }

        public bool AutoDelete { get; set; } = false;

        public bool Durable { get; set; } = true;

        public Dictionary<string, object> Args { get; set; } = [];

        public IEnumerable<ProvisioningBindingOptions> Bindings { get; set; } = [];
    }

    public class ProvisioningBindingOptions
    {
        [Required]
        public string ExchangeName { get; set; }

        public string RoutingKey { get; set; }

        public Dictionary<string, object> Args { get; set; } = [];
    }
}
