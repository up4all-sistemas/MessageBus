using Microsoft.Extensions.DependencyInjection;

using Up4All.Framework.MessageBus.Abstractions.Pipelines;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Pipelines
{
    public class RabbitMQMessageBusPipeline
        : MessageBusPipeline<RabbitMQMessageBusOptions>
    {
        private readonly RabbitMQMessageBusPublisherPipeline _producer;
        private readonly RabbitMQMessageBusStreamQueuePipeline _queue;
        private readonly RabbitMQMessageBusStreamQueuePipeline _stream;

        public RabbitMQMessageBusPipeline(IServiceCollection services, string configurationBindKey)
            : base(services, configurationBindKey)
        {
            _producer = new(this);
            _queue = new(this);
            _stream = new(this);
            AddOptions();
            _pipelines.Add(_producer);
            _pipelines.Add(_queue);
            _pipelines.Add(_stream);
        }

        public RabbitMQMessageBusStreamQueuePipeline Streams => _stream;

        public RabbitMQMessageBusStreamQueuePipeline Queues => _queue;

        public RabbitMQMessageBusPublisherPipeline Producers => _producer;

    }
}
