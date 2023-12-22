using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQTopicClient : MessageBusTopicClient, IRabbitMQClient, IMessageBusPublisher
    {
        private readonly ILogger<RabbitMQTopicClient> _logger;

        public RabbitMQTopicClient(ILogger<RabbitMQTopicClient> logger, IOptions<MessageBusOptions> messageOptions) : base(messageOptions)
        {
            _logger = logger;
        }

        public IConnection Connection { get; set; }

        public override async Task SendAsync<TModel>(TModel model, CancellationToken cancellation = default)
        {
            var message = model.CreateMessagebusMessage();
            await SendAsync(message, cancellation);
        }

        public override Task SendAsync(MessageBusMessage message, CancellationToken cancellation = default)
        {
            using (var channel = this.CreateChannel(this.GetConnection(MessageBusOptions, _logger)))
                channel.SendMessage(MessageBusOptions.TopicName, string.Empty, message, cancellation);

            return Task.CompletedTask;
        }

        public override Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellation = default)
        {
            using (var channel = this.CreateChannel(this.GetConnection(MessageBusOptions, _logger)))
            {
                foreach (var message in messages)
                    channel.SendMessage(MessageBusOptions.TopicName, string.Empty, message, cancellation);
            }

            return Task.CompletedTask;
        }

        public override async Task SendManyAsync<TModel>(IEnumerable<TModel> list, CancellationToken cancellation = default)
        {
            await SendAsync(list.Select(x => x.CreateMessagebusMessage()), cancellation);
        }
    }
}
