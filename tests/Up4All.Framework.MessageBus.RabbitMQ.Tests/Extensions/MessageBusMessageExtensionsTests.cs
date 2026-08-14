using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.RabbitMQ.Consts;
using Up4All.Framework.MessageBus.RabbitMQ.Extensions;

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests.Extensions
{
    [TestFixture]
    public class MessageBusMessageExtensionsTests
    {
        [Test]
        public void IsPersistent_True_AddsUserProperty()
        {
            var message = new MessageBusMessage();

            message.IsPersistent(true);

            Assert.That(message.UserProperties[Properties.IsPersistent], Is.EqualTo(true));
        }

        [Test]
        public void IsPersistent_False_RemovesUserProperty()
        {
            var message = new MessageBusMessage();
            message.IsPersistent(true);

            message.IsPersistent(false);

            Assert.That(message.UserProperties.ContainsKey(Properties.IsPersistent), Is.False);
        }

        [Test]
        public void IsPersistent_FalseWhenNotSet_DoesNotThrow()
        {
            var message = new MessageBusMessage();

            Assert.DoesNotThrow(() => message.IsPersistent(false));
        }
    }
}
