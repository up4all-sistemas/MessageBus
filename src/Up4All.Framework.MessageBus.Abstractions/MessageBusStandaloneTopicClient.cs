namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneTopicClient(string connectionString, string topicName) : MessageBusStandaloneClientBase
    {
        protected string ConnectionString { get; private set; } = connectionString;
        public string TopicName { get; private set; } = topicName;
    }
}
