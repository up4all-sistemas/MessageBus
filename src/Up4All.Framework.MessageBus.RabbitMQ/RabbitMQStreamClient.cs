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
    public class RabbitMQStreamClient : MessageBusStreamClient, IRabbitMQClient, IMessageBusStreamClient
    {
        public IConnection Connection { get; set; }
        public IModel Channel { get; private set; }

        public RabbitMQStreamClient(IOptions<MessageBusOptions> messageOptions, ILogger<RabbitMQStreamClient> logger, object offset
            , StreamDeclareOptions declareOpts = null) : base(messageOptions, offset)
        {
            this.GetConnection(MessageBusOptions, logger);
            Channel = this.CreateChannel();
            Channel.ConfigureQueueDeclare(MessageBusOptions.StreamName, declareOpts);
        }

        public override void RegisterHandler(Func<ReceivedMessage, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {

            var receiver = new QueueMessageReceiver(Channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(MessageBusOptions.StreamName, receiver, false, Offset);
        }

        public override void RegisterHandler<TModel>(Func<TModel, MessageReceivedStatus> handler, Action<Exception> errorHandler, Action onIdle = null, bool autoComplete = false)
        {

            var receiver = new QueueMessageReceiverForModel<TModel>(Channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(MessageBusOptions.StreamName, receiver, false, Offset);
        }

        public override Task RegisterHandlerAsync(Func<ReceivedMessage, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {

            var receiver = new QueueMessageReceiver(Channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(MessageBusOptions.StreamName, receiver, false, Offset);
            return Task.CompletedTask;
        }

        public override Task RegisterHandlerAsync<TModel>(Func<TModel, CancellationToken, Task<MessageReceivedStatus>> handler, Func<Exception, CancellationToken, Task> errorHandler, Func<CancellationToken, Task> onIdle = null, bool autoComplete = false, CancellationToken cancellationToken = default)
        {

            var receiver = new QueueMessageReceiverForModel<TModel>(Channel, handler, errorHandler, autoComplete);
            this.ConfigureHandler(MessageBusOptions.StreamName, receiver, false, Offset);
            return Task.CompletedTask;
        }

        private void CreateAndSend(MessageBusMessage msg)
        {
            IBasicProperties basicProps = Channel.CreateBasicProperties();
            basicProps.PopulateHeaders(msg);
            Channel.BasicPublish(exchange: "", routingKey: MessageBusOptions.StreamName, basicProperties: basicProps, body: msg.Body);
        }

        private void CreateAndSend<TModel>(TModel model)
        {
            IBasicProperties basicProps = Channel.CreateBasicProperties();

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(model, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

            Channel.BasicPublish(exchange: "", routingKey: MessageBusOptions.StreamName, basicProperties: basicProps, body: body);
        }

        public override Task Close()
        {
            Channel?.Close();
            return Task.CompletedTask;
        }

        public override Task SendAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            CreateAndSend(model);
            return Task.CompletedTask;
        }

        public override Task SendAsync(MessageBusMessage message, CancellationToken cancellationToken = default)
        {
            CreateAndSend(message);
            return Task.CompletedTask;
        }

        public override Task SendAsync(IEnumerable<MessageBusMessage> messages, CancellationToken cancellationToken = default)
        {
            foreach (var message in messages)
                CreateAndSend(message);

            return Task.CompletedTask;
        }

        public override Task SendManyAsync<TModel>(IEnumerable<TModel> models, CancellationToken cancellationToken = default)
        {
            foreach (var model in models)
                CreateAndSend(model);

            return Task.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            Close();
        }
    }
}
