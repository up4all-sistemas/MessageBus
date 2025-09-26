using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQQueueClient(IOptions<MessageBusOptions> messageOptions, QueueDeclareOptions declareOpts = null) : RabbitMQStandaloneQueueClient(messageOptions.Value.ConnectionString, messageOptions.Value.QueueName, messageOptions.Value.ConnectionAttempts, declareOpts), IMessageBusQueueClient
    {
    }

    public class RabbitMQQueueAsyncClient(IOptions<MessageBusOptions> messageOptions, QueueDeclareOptions declareOpts = null) : RabbitMQStandaloneQueueAsyncClient(messageOptions.Value.ConnectionString, messageOptions.Value.QueueName, messageOptions.Value.ConnectionAttempts, declareOpts), IMessageBusQueueAsyncClient
    {
    }
}
