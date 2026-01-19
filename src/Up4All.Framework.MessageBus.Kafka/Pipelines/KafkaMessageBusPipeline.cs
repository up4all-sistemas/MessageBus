using Microsoft.Extensions.DependencyInjection;

using Up4All.Framework.MessageBus.Abstractions.Pipelines;
using Up4All.Framework.MessageBus.Kafka.Options;

namespace Up4All.Framework.MessageBus.Kafka.Pipelines
{
    public class KafkaMessageBusPipeline
        : MessageBusPipeline<KafkaMessageBusOptions>
    {
        private readonly KafkaMessageBusSubscriptionPipeline _sub;
        private readonly KafkaMessageBusPublisherPipeline _producer;

        public KafkaMessageBusPipeline(IServiceCollection services, string configurationBindKey)
            : base(services, configurationBindKey)
        {
            _sub = new(this);
            _producer = new(this);
            AddOptions();
            _pipelines.Add(_sub);
            _pipelines.Add(_producer);
        }

        public KafkaMessageBusSubscriptionPipeline Subscriptions => _sub;

        public KafkaMessageBusPublisherPipeline Producers => _producer;
    }
}
