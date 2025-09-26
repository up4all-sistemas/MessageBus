namespace Up4All.Framework.MessageBus.Mocks
{
    public abstract class MessageBusClientBaseMock
    {
        public string TopicName { get; } = string.Empty;

        public string QueueName { get; }  = string.Empty;

        protected MessageBusClientBaseMock()
        {

        }
    }
}
