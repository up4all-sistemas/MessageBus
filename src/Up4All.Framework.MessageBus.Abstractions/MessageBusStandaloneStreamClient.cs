namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneStreamClient(string connectionString, string queueName, object offset, int connectionAttempts = 8) : MessageBusStandaloneClientBase
    {
        protected string ConnectionString { get; private set; } = connectionString;
        protected int ConnectionAttempts { get; private set; } = connectionAttempts;
        protected object Offset { get; } = offset;
        public string EntityPath { get; private set; } = queueName;
        public string TopicName { get; } = string.Empty;

    }
}
