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
    public class RabbitMQQueueClient : MessageBusQueueClient, IRabbitMQClient, IMessageBusQueueClient, IDisposable
    {
        private readonly object _offset;
        private readonly ILogger<RabbitMQQueueClient> _logger;
        private IModel _channel;

        public IConnection Connection { get; set; }

        public RabbitMQQueueClient(IOptions<MessageBusOptions> messageOptions, ILogger<RabbitMQQueueClient> logger, object offset = null
            , QueueDeclareOptions declareOpts = null) : base(messageOptions)
        {
            _offset = offset;
            _logger = logger;
            _channel = this.CreateChannel(this.GetConnection(MessageBusOptions, logger));
            _channel.ConfigureQueueDeclare(MessageBusOptions.QueueName, declareOpts);
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _channel = this.CreateChannel(this.GetConnection(MessageBusOptions, _logger));
            var receiver = new QueueMessageReceiver(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, MessageBusOptions.QueueName, receiver, autoComplete, _offset);
        }

        public override Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            _channel = this.CreateChannel(this.GetConnection(MessageBusOptions, _logger));
            var receiver = new QueueMessageReceiver(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, MessageBusOptions.QueueName, receiver, autoComplete, _offset);
            return Task.CompletedTask;
        }

        public override Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatusEnum>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {
            _channel = this.CreateChannel(this.GetConnection(MessageBusOptions, _logger));
            var receiver = new QueueMessageReceiverForModel<TModel>(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, MessageBusOptions.QueueName, receiver, autoComplete, _offset);
            return Task.CompletedTask;
        }

        public override void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatusEnum> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {
            _channel = this.CreateChannel(this.GetConnection(MessageBusOptions, _logger));
            var receiver = new QueueMessageReceiverForModel<TModel>(_channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(_channel, MessageBusOptions.QueueName, receiver, autoComplete, _offset);
        }

        private void CreateAndSend(MessageBusMessage msg)
        {
            IBasicProperties basicProps = _channel.CreateBasicProperties();
            basicProps.PopulateHeaders(msg);
            _channel.BasicPublish(exchange: "", routingKey: MessageBusOptions.QueueName, basicProperties: basicProps, body: msg.Body);
        }

        private void CreateAndSend<TModel>(TModel model)
        {
            IBasicProperties basicProps = _channel.CreateBasicProperties();

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(model, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

            _channel.BasicPublish(exchange: "", routingKey: MessageBusOptions.QueueName, basicProperties: basicProps, body: body);
        }

        public void Dispose()
        {
            Close();
        }

        public override Task Close()
        {
            _channel?.Close();
            //base.Close();

            return Task.CompletedTask;
        }

        public override Task SendAsync<TModel>(TModel model, CancellationToken cancellation = default)
        {
            CreateAndSend(model);
            return Task.CompletedTask;
        }

        public override Task SendManyAsync<TModel>(IEnumerable<TModel> list, CancellationToken cancellation = default)
        {
            foreach (var model in list)
                CreateAndSend(model);

            return Task.CompletedTask;
        }

        public override Task SendAsync(MessageBusMessage message, CancellationToken cancellation = default)
        {
            CreateAndSend(message);
            return Task.CompletedTask;
        }

        public override Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellation = default)
        {
            foreach (var message in messages)
                CreateAndSend(message);

            return Task.CompletedTask;
        }

    }
}
