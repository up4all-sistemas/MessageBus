
using RabbitMQ.Client;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Mocks;

namespace Up4All.Framework.MessageBus.RabbitMQ.Mocks
{
    public class RabbitMQTopicClientMocked : MessageBusTopicClientMock, IMessageBusPublisher, IRabbitMQClient
    {
        public RabbitMQTopicClientMocked() : base()
        {
        }

        public IConnection Connection { get; set; }
        public IModel Channel { get; private set; }

        public override Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
