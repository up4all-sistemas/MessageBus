using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.Abstractions.Factories
{
    public class MessageBusFactory(IServiceProvider provider)
    {
        private readonly IServiceProvider _provider = provider;

        public IMessageBusStandaloneQueueAsyncClient GetQueueAsyncClient(string key)
        {
            var namedInstances = _provider.GetServices<NamedInstanceClient<IMessageBusStandaloneQueueAsyncClient>>();
            return namedInstances.FirstOrDefault(x => x.Key == key)?.Instance;

        }

        public IMessageBusStandalonePublisherAsync GetTopicAsyncClient(string key)
        {
            var namedInstances = _provider.GetServices<NamedInstanceClient<IMessageBusStandalonePublisherAsync>>();
            return namedInstances.FirstOrDefault(x => x.Key == key)?.Instance;

        }

        public IMessageBusStandaloneAsyncConsumer GetSubscriptionAsyncClient(string key)
        {
            var namedInstances = _provider.GetServices<NamedInstanceClient<IMessageBusStandaloneAsyncConsumer>>();
            return namedInstances.FirstOrDefault(x => x.Key == key)?.Instance;

        }

        public IMessageBusStandaloneAsyncConsumer GetStreamAsyncClient(string key)
        {
            var namedInstances = _provider.GetServices<NamedInstanceClient<IMessageBusStandaloneAsyncConsumer>>();
            return namedInstances.FirstOrDefault(x => x.Key == key)?.Instance;

        }
    }
}
