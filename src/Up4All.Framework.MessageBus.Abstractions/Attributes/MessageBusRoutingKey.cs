using System;

namespace Up4All.Framework.MessageBus.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageBusRoutingKeyAttribute(string routingKey) : Attribute
    {
        public string RoutingKey => routingKey;
    }
}
