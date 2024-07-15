using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.Abstractions.Messages
{
    public class MessageBusMessage : IMessage
    {
        public byte[] Body { get; private set; }

        public IDictionary<string, object> UserProperties { get; private set; }

        public bool IsJson { get; private set; }

        public MessageBusMessage()
        {
            IsJson = false;
            UserProperties = new Dictionary<string, object>();
        }

        public MessageBusMessage(byte[] body) : this()
        {
            IsJson = false;
            Body = body;
        }

        public void AddBody(byte[] body)
        {
            Body = body;
        }

        public void AddBody(string body)
        {
            AddBody(Encoding.UTF8.GetBytes(body));
        }

        public void AddBody(BinaryData data, bool isJsonData = false)
        {
            IsJson = isJsonData;
            AddBody(data.ToArray());
        }

        public void AddBody<T>(T obj, JsonSerializerOptions opts = null) where T : class
        {
            opts = opts ?? new JsonSerializerOptions(JsonSerializerDefaults.Web) { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault };

            IsJson = true;
            AddBody(JsonSerializer.Serialize(obj, opts));
        }

        public void AddUserProperty(KeyValuePair<string, object> prop, bool replace = false)
        {
            if (UserProperties.ContainsKey(prop.Key) && !replace) return;

            UserProperties.Remove(prop.Key);
            UserProperties.Add(prop);
        }

        public void AddUserProperty(string key, object value, bool replace = false)
        {
            AddUserProperty(new KeyValuePair<string, object>(key, value), replace);
        }

        public void AddUserProperties(IDictionary<string, object> props, bool replace = false)
        {
            foreach (var prop in props)
                AddUserProperty(prop, replace);
        }

        public void RemoveUserProperty(string key)
        {
            if (UserProperties.ContainsKey(key))
                UserProperties.Remove(key);
        }
    }
}
