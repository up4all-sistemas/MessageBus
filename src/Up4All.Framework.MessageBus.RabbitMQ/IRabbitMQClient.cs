using RabbitMQ.Client;

namespace Up4All.Framework.MessageBus.RabbitMQ
{
    public interface IRabbitMQClient
    {
        IConnection Connection { get; set; }
    }
}
