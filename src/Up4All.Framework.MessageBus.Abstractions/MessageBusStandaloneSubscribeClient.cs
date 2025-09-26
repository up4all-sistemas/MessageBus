﻿namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneSubscribeClient(string connectionString, string topicName, string subscriptionName) : MessageBusStandaloneClientBase
    {
        protected string ConnectionString { get; private set; } = connectionString;
        protected string TopicName { get; private set; } = topicName;
        public string QueueName { get; private set; } = subscriptionName;
    }
}
