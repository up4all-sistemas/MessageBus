using System;
using System.Linq;
using System.Reflection;
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
    }
}
