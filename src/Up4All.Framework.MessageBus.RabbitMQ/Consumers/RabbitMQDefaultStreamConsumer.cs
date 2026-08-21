using Microsoft.Extensions.Logging;

using Up4All.Framework.MessageBus.Abstractions.Consumers;
using Up4All.Framework.MessageBus.Abstractions.Handlers;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.RabbitMQ.Consumers
{
    public class RabbitMQDefaultStreamConsumer<THandler>(IMessageBusStreamAsyncClient consumer, IMessageBusMessageHandler handler, ILogger<DefaultConsumer<THandler>> logger)
        : DefaultConsumer<THandler>(consumer, handler, logger)
        where THandler : class, IMessageBusMessageHandler
    {
    }
}
