using System;
using System.Diagnostics;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Kafka.Extensions
{
    public static class MessageBusStandaloneTopicClientExtensions
    {
        public static void AddActivityTrace(this MessageBusStandaloneTopicClient client, MessageBusMessage message)
        {
            var activityName = $"message-send {client.TopicName}";

            using var activity = KafkaExtensions.ActivitySource.ProcessOpenTelemetryActivity(activityName, ActivityKind.Producer);
            activity?.InjectPropagationContext(message.UserProperties);
            activity?.AddTagsToActivity("servicebus", message.Body, client.TopicName);
        }
    }
}
