using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;

using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQTopicAsyncClient(ILogger<RabbitMQTopicAsyncClient> logger, IOptions<MessageBusOptions> messageOptions, string type = ExchangeType.Topic
            , ExchangeDeclareOptions declareOpts = null)
        : RabbitMQStandaloneTopicAsyncClient(logger, messageOptions.Value.ConnectionString, messageOptions.Value.TopicName, messageOptions.Value.ConnectionAttempts, type, declareOpts)
    {
    }
}
