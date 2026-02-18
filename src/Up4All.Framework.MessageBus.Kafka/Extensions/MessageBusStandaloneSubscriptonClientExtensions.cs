using System.Diagnostics;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Kafka.Extensions
{
    public static class MessageBusStandaloneSubscriptonClientExtensions
    {
        public static Activity? AddActivityTrace<TSource>(this MessageBusStandaloneSubscriptonClient client, ReceivedMessage message, object mesageId)
            where TSource : class
        {
            var activity = message.CreateMessageReceivedActivity<TSource>(client.EntityPath);
            activity?.InjectPropagationContext(message.UserProperties);
            activity?.AddTagsToActivity("kafka", message, client.EntityPath, mesageId);

            return activity;
        }

        private static string CreateActivityName(this string entityPath)
        {
            return $"{entityPath} receive";
        }

        public static Activity CreateMessageReceivedActivity<TSource>(this ReceivedMessage message, string entityPath)
            where TSource : class
        {
            var activityName = entityPath.CreateActivityName();
            return message.UserProperties.CreateActivity<TSource>(activityName, ActivityKind.Consumer);
        }
    }
}
