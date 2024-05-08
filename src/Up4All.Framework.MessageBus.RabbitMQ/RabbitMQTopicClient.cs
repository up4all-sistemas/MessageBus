using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQTopicClient : RabbitMQStandaloneTopicClient
    {
        public RabbitMQTopicClient(IOptions<MessageBusOptions> messageOptions, string type = "topic"
            , ExchangeDeclareOptions declareOpts = null) : base(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName, messageOptions.Value.ConnectionAttempts, type, declareOpts)
        {
        }
    }
}
