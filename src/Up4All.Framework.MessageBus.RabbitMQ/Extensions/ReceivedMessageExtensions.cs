using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Consts;

namespace Up4All.Framework.MessageBus.RabbitMQ.Extensions
{
    internal static class ReceivedMessageExtensions
    {
        internal static void PopulateUserProperties(this ReceivedMessage message, IReadOnlyBasicProperties properties)
        {
            if (!properties.IsHeadersPresent()) return;

            if (!string.IsNullOrEmpty(properties.CorrelationId)
                && Guid.TryParse(properties.CorrelationId, out var correlationId))
                message.SetCorrelationId(correlationId);

            var headers = properties.Headers;
            if (headers is null) return;

            foreach (var prop in headers)
            {
                var value = ConvertPropertyValue(prop.Value);
                if (value is not null)
                    message.AddUserProperty(prop.Key, value);
            }
        }

        internal static void PopulateHeaders(this IBasicProperties properties, MessageBusMessage message)
        {
            if (!message.UserProperties.Any()) return;

            var headers = new Dictionary<string, object?>();
            foreach (var prop in message.UserProperties)
                headers.Add(prop.Key, prop.Value);

            properties.Headers = headers;

            properties.MessageId ??= message.GetMessageIdForClass<string>() ?? Guid.NewGuid().ToString("N");
        }

        internal static ReceivedMessage CreateReceivedMessage(this ReadOnlyMemory<byte> body, IReadOnlyBasicProperties properties)
        {
            var message = new ReceivedMessage();
            message.AddBody(BinaryData.FromBytes(body), true);
            message.PopulateUserProperties(properties);
            return message;
        }

        internal static object? ConvertPropertyValue(object? rawValue)
        {
            if (rawValue is byte[] bytedValue)
                return Encoding.UTF8.GetString(bytedValue);
            else
                return rawValue;
        }
        
    }
}
