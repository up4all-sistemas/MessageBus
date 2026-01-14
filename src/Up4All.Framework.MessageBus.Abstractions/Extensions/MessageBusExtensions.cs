using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

using Up4All.Framework.MessageBus.Abstractions.Attributes;

using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Abstractions.Extensions
{
    public static class MessageBusExtensions
    {
        public static MessageBusMessage CreateMessagebusMessage<TModel>(this TModel model)
        {
            var message = new MessageBusMessage();
            var modelType = model.GetType();
            var target = modelType.GetCustomAttribute<MessageBusPayloadAttribute>();
            var routingKey = modelType.GetCustomAttribute<MessageBusRoutingKeyAttribute>();
            message.AddUserProperties(modelType.GetCustomAttributes<MessageBusAdditionalUserPropertyAttribute>().ToDictionary(x => x.Key, x => x.Value));

            if (!string.IsNullOrEmpty(target?.Target))
                message.AddUserProperty("target", target.Target);

            if (!string.IsNullOrEmpty(routingKey?.RoutingKey))
                message.AddRoutingKey(routingKey.RoutingKey);

            var properties = modelType.GetProperties();

            foreach (var prop in properties)
            {
                if (!prop.CustomAttributes.Any(x => x.AttributeType == typeof(MessageBusUserPropertyAttribute))) continue;

                var attr = prop.GetCustomAttribute<MessageBusUserPropertyAttribute>();
                if (attr is null) continue;

                message.AddUserProperty(attr.Key, prop.GetValue(model, null));
            }

            message.AddBody(BinaryData.FromString(JsonSerializer.Serialize(model, new JsonSerializerOptions(JsonSerializerDefaults.Web))));
            return message;
        }

        public static void AddRoutingKey(this MessageBusMessage message, string routingKey)
        {
            message.AddUserProperty("routing-key", routingKey, true);
        }

        public static bool ContainsRoutingKey(this MessageBusMessage message)
        {
            return message.UserProperties.ContainsKey("routing-key");
        }

        public static string GetRoutingKey(this MessageBusMessage message)
        {
            return message.UserProperties["routing-key"].ToString();
        }

        public static string GetUserPropertyAsString(this MessageBusMessage message, string userPropertyKey, string defaultValue = default)
        {
            message.UserProperties.TryGetValue(userPropertyKey, out var rawValue);
            if (rawValue == null) return defaultValue;
            return Encoding.UTF8.GetString((byte[])rawValue);
        }

        public static bool TryGetUserPropertyAsString(this MessageBusMessage message, string userPropertyKey, out string value)
        {
            value = default;
            if (message.UserProperties.TryGetValue(userPropertyKey, out var rawValue))
            {
                value = Encoding.UTF8.GetString((byte[])rawValue);
                return true;
            }
            return false;
        }

        public static bool TryGetUserPropertyAsInt32(this MessageBusMessage message, string userPropertyKey, out int value)
        {
            value = default;
            return message.TryGetUserPropertyAsString(userPropertyKey, out var valueStr) && int.TryParse(valueStr, out value);
        }

        public static bool TryGetUserPropertyAsDecimal(this MessageBusMessage message, string userPropertyKey, out decimal value)
        {
            value = default;
            return message.TryGetUserPropertyAsString(userPropertyKey, out var valueStr) && decimal.TryParse(valueStr, out value);
        }

        public static bool TryGetUserPropertyAsDateTime(this MessageBusMessage message, string userPropertyKey, out DateTime value)
        {
            value = default;
            return message.TryGetUserPropertyAsString(userPropertyKey, out var valueStr) && DateTime.TryParse(valueStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
        }

        public static bool TryGetUserPropertyAsDateTime(this MessageBusMessage message, IFormatProvider formatProvider, DateTimeStyles dateTimeStyles, string userPropertyKey, out DateTime value)
        {
            value = default;
            return message.TryGetUserPropertyAsString(userPropertyKey, out var valueStr) && DateTime.TryParse(valueStr, formatProvider, dateTimeStyles, out value);
        }

        public static bool TryGetUserPropertyAs<T>(this MessageBusMessage message, string userPropertyKey, out T value) where T : class
        {
            value = default;
            if (message.TryGetUserPropertyAsString(userPropertyKey, out var valueStr))
            {
                try
                {
                    value = JsonSerializer.Deserialize<T>(valueStr, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                    return true;
                }
                catch (JsonException)
                {
                    // Handle deserialization failure
                    return false;
                }
            }

            return false;
        }
    }
}
