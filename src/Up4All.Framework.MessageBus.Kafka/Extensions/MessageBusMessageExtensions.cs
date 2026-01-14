using Confluent.Kafka;

using System.Linq;
using System.Text.Json;

using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Kafka.Extensions
{
    public static class MessageBusMessageExtensions
    {
        public static Message<TMessageKey, byte[]> ToKafkaMessage<TMessageKey>(this MessageBusMessage data)
            where TMessageKey : class
        {
            var message = new Message<TMessageKey, byte[]>
            {
                Key = data.GetMessageId<TMessageKey>(),
                Value = data.Body
            };

            if (data.UserProperties.Any()) message.Headers = [];
            foreach (var prop in data.UserProperties)
                message.Headers.Add(prop.Key, JsonSerializer.SerializeToUtf8Bytes(prop.Value));

            return message;
        }

        public static Message<TMessageKey, byte[]> ToKafkaMessageFromKeyStruct<TMessageKey>(this MessageBusMessage data)
            where TMessageKey : struct
        {
            var message = new Message<TMessageKey, byte[]>
            {
                Key = data.GetMessageIdForStruct<TMessageKey>(),
                Value = data.Body
            };

            if (data.UserProperties.Any()) message.Headers = [];
            foreach (var prop in data.UserProperties)
                message.Headers.Add(prop.Key, JsonSerializer.SerializeToUtf8Bytes(prop.Value));

            return message;
        }
    }
}
