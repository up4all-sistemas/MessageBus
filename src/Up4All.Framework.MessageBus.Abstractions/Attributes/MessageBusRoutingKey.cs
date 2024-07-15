using System;

namespace Up4All.Framework.MessageBus.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageBusRoutingKey : Attribute
    {
        public string RoutingKey { get; private set; }

        public MessageBusRoutingKey(string routingKey)
        {
            RoutingKey = routingKey;
        }
    }
}
