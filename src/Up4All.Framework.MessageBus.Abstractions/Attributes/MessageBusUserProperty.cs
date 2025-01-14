using System;

namespace Up4All.Framework.MessageBus.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MessageBusUserPropertyAttribute(string key) : Attribute
    {
        public string Key => key;
    }
}
