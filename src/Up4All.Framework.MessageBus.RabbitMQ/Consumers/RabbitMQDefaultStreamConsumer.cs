using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.RabbitMQ.Consumers
{
    public class RabbitMQDefaultStreamConsumer(IMessageBusStreamAsyncClient consumer, IMessageBusMessageHandler handler)
        : RabbitMQDefaultConsumer(consumer, handler)
    {
    }
}
