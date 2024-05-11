namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneQueueClient : MessageBusStandaloneClientBase
    {
        protected string ConnectionString { get; private set; }
        protected string QueueName { get; private set; }

        protected MessageBusStandaloneQueueClient(string connectionString, string queueName)
        {
            ConnectionString = connectionString;
            QueueName = queueName;
        }

    }
}
