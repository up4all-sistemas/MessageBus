using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ
{

    public class RabbitMQStreamAsyncClient(IOptions<MessageBusOptions> messageOptions, object offset
            , StreamDeclareOptions declareOpts = null) : RabbitMQStandaloneStreamAsyncClient(messageOptions.Value.ConnectionString, messageOptions.Value.StreamName, offset, messageOptions.Value.ConnectionAttempts, declareOpts), IMessageBusStreamAsyncClient
    {
    }
}
