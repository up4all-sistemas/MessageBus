namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneTopicClient : MessageBusStandaloneClientBase
    {
        protected string ConnectionString { get; private set; }
        public string TopicName { get; private set; }

        protected MessageBusStandaloneTopicClient(string connectionString, string topicName)
        {
            ConnectionString = connectionString;
            TopicName = topicName;
        }
    }
}
