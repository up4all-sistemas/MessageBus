using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQQueueAsyncClient(ILogger<RabbitMQQueueAsyncClient> logger, IOptions<MessageBusOptions> messageOptions, QueueDeclareOptions declareOpts = null)
        : RabbitMQStandaloneQueueAsyncClient(logger, messageOptions.Value.ConnectionString, messageOptions.Value.QueueName, messageOptions.Value.ConnectionAttempts, declareOpts)
        , IMessageBusQueueAsyncClient
    {
    }
}
