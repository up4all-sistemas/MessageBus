namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneTopicClient(string connectionString, string topicName, int connectionAttempts = 8) : MessageBusStandaloneClientBase
    {
        protected string ConnectionString { get; private set; } = connectionString;
        protected int ConnectionAttempts { get; private set; } = connectionAttempts;
        public string TopicName { get; private set; } = topicName;
    }
}
