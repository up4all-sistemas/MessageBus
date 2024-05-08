using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Enums;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Consumers;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;
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
}
