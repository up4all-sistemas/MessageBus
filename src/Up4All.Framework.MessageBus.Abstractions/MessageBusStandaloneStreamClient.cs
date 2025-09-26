namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneStreamClient(string connectionString, string queueName, object offset) : MessageBusStandaloneClientBase
    {
        protected string ConnectionString { get; private set; } = connectionString;
        public string QueueName { get; private set; } = queueName;
        public string TopicName { get; } = string.Empty;

        protected object Offset = offset;
    }
}
