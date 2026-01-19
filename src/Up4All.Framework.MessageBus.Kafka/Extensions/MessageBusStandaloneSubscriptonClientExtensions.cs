using System.Diagnostics;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Kafka.Extensions
{
    public static class MessageBusStandaloneSubscriptonClientExtensions
    {
        public static void AddActivityTrace<TSource>(this MessageBusStandaloneSubscriptonClient client, ReceivedMessage message)
            where TSource : class
        {
            var activity = message.CreateMessageReceivedActivity<TSource>(client.EntityPath);
            activity?.InjectPropagationContext(message.UserProperties);
            activity?.AddTagsToActivity("kafka", message.Body, client.EntityPath);
        }

        private static string CreateActivityName(this ReceivedMessage message, string activityName, string entityPath)
        {
            return $"{activityName} {entityPath}";
        }

        private static string CreateMessageReceivedActivityName(this ReceivedMessage message, string entityPath)
        {
            return message.CreateActivityName("message-received", entityPath);
        }

        public static Activity CreateMessageReceivedActivity<TSource>(this ReceivedMessage message, string entityPath)
            where TSource : class
        {
            var activityName = message.CreateMessageReceivedActivityName(entityPath);
            return message.UserProperties.CreateActivity<TSource>(activityName, ActivityKind.Consumer);
        }
    }
}
