using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ
{

    public class RabbitMQStreamAsyncClient(ILogger<RabbitMQStreamAsyncClient> logger, IOptions<RabbitMQMessageBusOptions> messageOptions, object offset
            , StreamDeclareOptions declareOpts = null) : RabbitMQStandaloneStreamAsyncClient(logger, messageOptions.Value.ConnectionString, messageOptions.Value.StreamName, offset
            , messageOptions.Value.PersistentMessages, messageOptions.Value.ConnectionAttempts, declareOpts)
        , IMessageBusStreamAsyncClient
    {
    }
}
