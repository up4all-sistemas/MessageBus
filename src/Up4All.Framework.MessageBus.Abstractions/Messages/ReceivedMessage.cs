using System.Text;
using System.Text.Json;

using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.Abstractions.Messages
{
    public class ReceivedMessage : MessageBusMessage, IReceivedMessage
    {
        public string GetBody()
        {
            return Encoding.UTF8.GetString(Body);
        }

        public T GetBody<T>(JsonSerializerOptions opts = null)
        {
            opts ??= new JsonSerializerOptions(JsonSerializerDefaults.Web) { IncludeFields = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault };

            return JsonSerializer.Deserialize<T>(GetBody(), opts);
        }

        public object GetUserPropertyValue(string key)
        {
            if (!UserProperties.TryGetValue(key, out var val)) return null;
            return val;
        }

        public string GetUserPropertyValueAsString(string key, string defaultValue = default)
        {
            return this.GetUserPropertyAsString(key, defaultValue);
        }
    }
}
