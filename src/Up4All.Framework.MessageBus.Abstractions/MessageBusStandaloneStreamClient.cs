namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneStreamClient : MessageBusStandaloneClientBase
    {
        protected string ConnectionString { get; private set; }
        public string QueueName { get; private set; }
        public string TopicName => string.Empty;

        protected object Offset;

        protected MessageBusStandaloneStreamClient(string connectionString, string queueName, object offset)
        {
            ConnectionString = connectionString;
            QueueName = queueName;
            Offset = offset;
        }
    }
}
