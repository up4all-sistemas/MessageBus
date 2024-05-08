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
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQTopicClient : MessageBusTopicClient, IRabbitMQClient, IMessageBusPublisher
    {
        public RabbitMQTopicClient(IOptions<MessageBusOptions> messageOptions, string type = "topic"
            , ExchangeDeclareOptions declareOpts = null) : base(messageOptions)
        {
            this.GetConnection(messageOptions.Value.ConnectionString, messageOptions.Value.ConnectionAttempts);
            Channel = this.CreateChannel();
            if (declareOpts != null) Channel.ExchangeDeclare(messageOptions.Value.TopicName, type, declareOpts.Durable, declareOpts.AutoDelete, declareOpts.Args);
        }

        public IConnection Connection { get; set; }

        public IModel Channel { get; private set; }

        public override async Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            var message = model.CreateMessagebusMessage();
            await SendAsync(message, cancellationToken);
        }

        public override Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            Channel.SendMessage(MessageBusOptions.TopicName, string.Empty, message, cancellationToken);
            return Task.CompletedTask;
        }

        public override Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {

            foreach (var message in messages)
                Channel.SendMessage(MessageBusOptions.TopicName, string.Empty, message, cancellationToken);

            return Task.CompletedTask;
        }

        public override async Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
        {
            await SendAsync(models.Select(x => x.CreateMessagebusMessage()), cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            Connection.Close();
            Channel.Close();
        }
    }
}
