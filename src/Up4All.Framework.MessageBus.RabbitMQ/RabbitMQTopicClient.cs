using Microsoft.Extensions.Options;

using RabbitMQ.Client;

using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQTopicClient(IOptions<MessageBusOptions> messageOptions, string type = ExchangeType.Topic
            , ExchangeDeclareOptions declareOpts = null) : RabbitMQStandaloneTopicClient(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName, messageOptions.Value.ConnectionAttempts, type, declareOpts)
    {
    }

    public class RabbitMQTopicAsyncClient(IOptions<MessageBusOptions> messageOptions, string type = ExchangeType.Topic
            , ExchangeDeclareOptions declareOpts = null) : RabbitMQStandaloneTopicAsyncClient(messageOptions.Value.ConnectionString, messageOptions.Value.TopicName, messageOptions.Value.ConnectionAttempts, type, declareOpts)
    {
    }
}
