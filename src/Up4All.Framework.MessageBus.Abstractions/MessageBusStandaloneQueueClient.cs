namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneQueueClient(string connectionString, string queueName, int connectionAttempts = 8) : MessageBusStandaloneClientBase
    {
        protected string ConnectionString { get; private set; } = connectionString;
        protected int ConnectionAttempts { get; private set; } = connectionAttempts;
        public string EntityPath { get; private set; } = queueName;
        public string TopicName { get; } = string.Empty;
    }
}
