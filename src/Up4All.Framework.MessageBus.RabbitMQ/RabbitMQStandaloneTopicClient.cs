using RabbitMQ.Client;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions;
using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Interfaces;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public class RabbitMQStandaloneTopicClient : MessageBusStandaloneTopicClient, IRabbitMQClient, IMessageBusStandalonePublisher
    {
        private readonly string _topicName;

        public IConnection Connection { get; set; }

        public IModel Channel { get; private set; }

        public RabbitMQStandaloneTopicClient(string connectionString, string topicName, int connectionAttemps = 8, string type = "topic"
            , ExchangeDeclareOptions declareOpts = null) : base(connectionString, topicName)
        {
            _topicName = topicName;
            this.GetConnection(connectionString, connectionAttemps);
            Channel = this.CreateChannel();

            if (declareOpts != null) Channel.ExchangeDeclare(topicName, type, declareOpts.Durable, declareOpts.AutoDelete, declareOpts.Args);
        }

        public void Send<TModel>(TModel model)
        {
            var message = model.CreateMessagebusMessage();
            Send(message);
        }
        public void Send(MessageBusMessage message)
        {
            Channel.SendMessage(_topicName, string.Empty, message);
        }
        public void Send(IEnumerable<MessageBusMessage> messages)
        {
            foreach (var message in messages)
                Channel.SendMessage(_topicName, string.Empty, message);
        }
        public void SendMany<TModel>(IEnumerable<TModel> models)
        {
            Send(models.Select(x => x.CreateMessagebusMessage()));
        }
        protected override void Dispose(bool disposing)
        {
            Channel.Close();
            Connection.Close();
        }
    }
}
