using Azure.Messaging.ServiceBus;

using System;
using System.Collections.Generic;
using System.Text;

namespace Up4All.Framework.MessageBus.ServiceBus.Extensions
{
    public static class ServiceBusExtensions
    {
        internal static ServiceBusProcessor CreateTopicProcessor(this ServiceBusClient _client, string topicName, string subscriptionName, bool autoComplete)
        {
            return _client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = autoComplete,
                MaxConcurrentCalls = 1,
                MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(1),
                PrefetchCount = 0,
            });
        }

        internal static ServiceBusProcessor CreateQueueProcessor(this ServiceBusClient _client, string queueName, bool autoComplete)
        {
            return _client.CreateProcessor(queueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = autoComplete,
                MaxConcurrentCalls = 1,
                MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(1),
                PrefetchCount = 0,
            });
        }
    }
}
