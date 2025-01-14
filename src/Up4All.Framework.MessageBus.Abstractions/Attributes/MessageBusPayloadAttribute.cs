using System;

namespace Up4All.Framework.MessageBus.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MessageBusPayloadAttribute(string target) : Attribute
    {
        public string Target => target;
    }
}
