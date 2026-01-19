using Confluent.Kafka;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Kafka.Extensions;
using Up4All.Framework.MessageBus.Kafka.Interfaces;

namespace Up4All.Framework.MessageBus.Kafka
{
    public class KafkaStandaloneGenericTopicAsyncClient<TMessageKey> : MessageBusStandaloneTopicClient, IMessageBusStandalonePublisherAsync, IKafkaTopicClient
        where TMessageKey : class
    {
        private readonly IProducer<TMessageKey, byte[]> _producer;

        public KafkaStandaloneGenericTopicAsyncClient(string connectionString, string topicName, int connectionAttempts = 8) : base(connectionString, topicName, connectionAttempts)
        {
            _producer = this.CreateProducer<TMessageKey>(connectionString);
        }

        public async Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            var mb = model.CreateMessagebusMessage();
            await SendAsync(mb, cancellationToken);
        }

        public async Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            this.AddActivityTrace(message);
            await _producer.ProduceAsync(TopicName, message.ToKafkaMessage<TMessageKey>(), cancellationToken);
        }

        public async Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            foreach (var message in messages)
                await SendAsync(message, cancellationToken);
        }

        public async Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
        {
            foreach (var model in models)
                await SendAsync(model, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            _producer.Dispose();
        }
    }

    public class KafkaStandaloneWithStructKeyTopicAsyncClient<TMessageKey> : MessageBusStandaloneTopicClient, IMessageBusStandalonePublisherAsync, IKafkaTopicClient
        where TMessageKey : struct
    {
        private readonly IProducer<TMessageKey, byte[]> _producer;

        public KafkaStandaloneWithStructKeyTopicAsyncClient(string connectionString, string topicName, int connectionAttempts = 8) : base(connectionString, topicName, connectionAttempts)
        {
            _producer = this.CreateProducerForStructKey<TMessageKey>(connectionString);
        }

        public async Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            var mb = model.CreateMessagebusMessage();
            await SendAsync(mb, cancellationToken);
        }

        public async Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            this.AddActivityTrace(message);
            await _producer.ProduceAsync(TopicName, message.ToKafkaMessageFromKeyStruct<TMessageKey>(), cancellationToken);
        }

        public async Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            foreach (var message in messages)
                await SendAsync(message, cancellationToken);
        }

        public async Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
        {
            foreach (var model in models)
                await SendAsync(model, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            _producer.Dispose();
        }
    }


}
