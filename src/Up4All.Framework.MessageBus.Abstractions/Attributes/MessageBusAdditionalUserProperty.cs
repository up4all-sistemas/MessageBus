using System;

namespace Up4All.Framework.MessageBus.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MessageBusAdditionalUserProperty : Attribute
    {
        public string Key { get; private set; }

        public object Value { get; private set; }

        public MessageBusAdditionalUserProperty(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }
}
