using Microsoft.Extensions.Options;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQStreamClient : RabbitMQStandaloneStreamClient, IMessageBusStreamClient
    {
        public RabbitMQStreamClient(IOptions<MessageBusOptions> messageOptions, object offset
            , StreamDeclareOptions declareOpts = null) : base(messageOptions.Value.ConnectionString, messageOptions.Value.StreamName, offset, messageOptions.Value.ConnectionAttempts, declareOpts)
        {
        }
    }

    public class RabbitMQStreamAsyncClient : RabbitMQStandaloneStreamAsyncClient, IMessageBusStreamAsyncClient
    {
        public RabbitMQStreamAsyncClient(IOptions<MessageBusOptions> messageOptions, object offset
            , StreamDeclareOptions declareOpts = null) : base(messageOptions.Value.ConnectionString, messageOptions.Value.StreamName, offset, messageOptions.Value.ConnectionAttempts, declareOpts)
        {
        }
    }
}
