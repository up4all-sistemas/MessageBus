using System;

namespace Up4All.Framework.MessageBus.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MessageBusUserProperty : Attribute
    {
        public string Key { get; set; }

        public MessageBusUserProperty(string key = null)
        {
            Key = key;
        }
    }
}
