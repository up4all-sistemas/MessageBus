
using RabbitMQ.Client;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Mocks;

namespace Up4All.Framework.MessageBus.RabbitMQ.Mocks
{
    public class RabbitMQStandaloneTopicClientMocked : MessageBusStandaloneTopicClientMock, IMessageBusPublisher, IRabbitMQClient
    {
        public IConnection Connection { get; set; }

        public RabbitMQStandaloneTopicClientMocked() : base()
        {
        }

        public override Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override Task SendAsync<TModel>(TModel model, CancellationToken cancellation = default)
        {
            return Task.CompletedTask;
        }

        public override Task SendManyAsync<TModel>(IEnumerable<TModel> list, CancellationToken cancellation = default)
        {
            return Task.CompletedTask;
        }
    }
}
