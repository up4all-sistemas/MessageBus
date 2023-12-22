using RabbitMQ.Client;

using System.Collections.Generic;
using System.Linq;

using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.RabbitMQ.Extensions
{
    internal static class ReceivedMessageExtensions
    {
        internal static void PopulateUserProperties(this ReceivedMessage message, IDictionary<string, object> headers)
        {
            if (headers != null)
                foreach (var prop in headers)
                    message.AddUserProperty(prop.Key, prop.Value);
        }

        internal static void PopulateHeaders(this IBasicProperties properties, MessageBusMessage message)
        {
            if (!message.UserProperties.Any()) return;

            properties.Headers = new Dictionary<string, object>();
            foreach (var prop in message.UserProperties)
                properties.Headers.Add(prop);
        }
    }
}
