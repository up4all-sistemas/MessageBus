namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneSubscribeClient : MessageBusStandaloneClientBase
    {
        protected string ConnectionString { get; private set; }
        protected string TopicName { get; private set; }
        public string QueueName { get; private set; }

        protected MessageBusStandaloneSubscribeClient(string connectionString, string topicName, string subscriptionName)
        {
            ConnectionString = connectionString;
            TopicName = topicName;
            QueueName = subscriptionName;
        }
    }
}
