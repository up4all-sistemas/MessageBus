using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQQueueClient : RabbitMQStandaloneQueueClient, IMessageBusQueueClient
    {
        public RabbitMQQueueClient(IOptions<MessageBusOptions> messageOptions, QueueDeclareOptions declareOpts = null)
            : base(messageOptions.Value.ConnectionString, messageOptions.Value.QueueName, messageOptions.Value.ConnectionAttempts, declareOpts)
        {
        }
    }
}
