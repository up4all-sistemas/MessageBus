namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneSubscriptonClient(string connectionString, string topicName, string subscriptionName) : MessageBusStandaloneClientBase
    {
        protected string ConnectionString { get; private set; } = connectionString;
        protected string TopicName { get; private set; } = topicName;
        public string EntityPath { get; private set; } = subscriptionName;
    }
}
