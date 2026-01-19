using Microsoft.Extensions.DependencyInjection;

using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.Abstractions.Pipelines;

namespace Up4All.Framework.MessageBus.ServiceBus.Pipelines
{
    public class ServiceBusMessageBusPipeline
        : MessageBusPipeline<MessageBusOptions>
    {
        private readonly ServiceBusMessageBusQueuePipeline _queues;
        private readonly ServiceBusMessageBusSubscriptionPipeline _subs;
        private readonly ServiceBusMessageBusPublisherPipeline _producers;


        public ServiceBusMessageBusPipeline(IServiceCollection services, string configurationBindKey)
            : base(services, configurationBindKey)
        {
            _queues = new ServiceBusMessageBusQueuePipeline(this);
            _subs = new ServiceBusMessageBusSubscriptionPipeline(this);
            _producers = new ServiceBusMessageBusPublisherPipeline(this);

            AddOptions();
            _pipelines.Add(_queues);
            _pipelines.Add(_subs);
            _pipelines.Add(_producers);
        }

        public ServiceBusMessageBusQueuePipeline Queues => _queues;

        public ServiceBusMessageBusSubscriptionPipeline Subscriptions => _subs;

        public ServiceBusMessageBusPublisherPipeline Producers => _producers;

    }
}
