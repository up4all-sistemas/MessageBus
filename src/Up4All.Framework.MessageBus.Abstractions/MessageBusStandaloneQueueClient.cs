namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneQueueClient : MessageBusStandaloneClientBase
    {
        protected string ConnectionString { get; private set; }
        public string QueueName { get; private set; }
        public string TopicName => string.Empty;

        protected MessageBusStandaloneQueueClient(string connectionString, string queueName)
        {
            ConnectionString = connectionString;
            QueueName = queueName;
        }

    }
}
