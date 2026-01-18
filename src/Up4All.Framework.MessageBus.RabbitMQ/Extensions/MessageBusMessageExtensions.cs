using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Enums;

namespace Up4All.Framework.MessageBus.RabbitMQ.Extensions
{
    public static class MessageBusMessageExtensions
    {
        public static void IsPersistent(this MessageBusMessage message, bool value)
        {
            if (value)
                message.AddUserProperty(Properties.IsPersistent, true);
            else
                message.RemoveUserProperty(Properties.IsPersistent);

        }
    }
}
