using System;

namespace Up4All.Framework.MessageBus.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MessageBusPayload : Attribute
    {
        public string Target { get; private set; }

        public MessageBusPayload(string target = null)
        {
            Target = target;
        }
    }
}
