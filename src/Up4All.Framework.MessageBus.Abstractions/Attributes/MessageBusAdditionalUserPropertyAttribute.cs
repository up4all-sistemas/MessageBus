using System;

namespace Up4All.Framework.MessageBus.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MessageBusAdditionalUserPropertyAttribute(string key, object value) : Attribute
    {
        public string Key => key;

        public object Value => value;

    }
}
