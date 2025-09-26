namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneQueueClient(string connectionString, string queueName) : MessageBusStandaloneClientBase
    {
        protected string ConnectionString { get; private set; } = connectionString;
        public string QueueName { get; private set; } = queueName;
        public string TopicName { get; } = string.Empty;
    }
}
