using System;
using System.Collections.Generic;
using System.Diagnostics;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Kafka.Extensions
{
    public static class MessageBusStandaloneTopicClientExtensions
    {
        public static void AddActivityTrace(this MessageBusStandaloneTopicClient client, MessageBusMessage message, object messageId)
        {
            var activityName = $"{client.TopicName} send";

            var additionalArgs = new Dictionary<string, object>();

            if (messageId is not null)
                additionalArgs.Add("messaging.kafka.message.key", messageId);

            using var activity = KafkaExtensions.ActivitySource.ProcessOpenTelemetryActivity(activityName, ActivityKind.Producer);
            activity?.InjectPropagationContext(message.UserProperties);
            activity?.AddTagsToActivity("kafka", message, client.TopicName, messageId, operationType: "send", additionalTags: additionalArgs);
        }
    }
}
