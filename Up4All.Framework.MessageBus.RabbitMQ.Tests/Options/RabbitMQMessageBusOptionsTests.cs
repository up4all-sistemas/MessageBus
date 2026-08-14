using Up4All.Framework.MessageBus.RabbitMQ.Consts;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests.Options
{
    [TestFixture]
    public class RabbitMQMessageBusOptionsTests
    {
        [Test]
        public void Defaults_PersistentMessagesTrue_ProvisioningNotProvided()
        {
            var options = new RabbitMQMessageBusOptions();

            Assert.That(options.PersistentMessages, Is.True);
            Assert.That(options.ProvisioningOptions, Is.Null);
            Assert.That(options.ProvisioningProvided, Is.False);
        }

        [Test]
        public void ProvisioningProvided_TrueWhenProvisioningOptionsSet()
        {
            var options = new RabbitMQMessageBusOptions
            {
                ProvisioningOptions = new ProvisioningOptions()
            };

            Assert.That(options.ProvisioningProvided, Is.True);
        }
    }

    [TestFixture]
    public class ProvisioningOptionsTests
    {
        [Test]
        public void Defaults_ClassicDurableNoArgsNoBindings()
        {
            var options = new ProvisioningOptions();

            Assert.That(options.Type, Is.EqualTo(QueueType.Classic));
            Assert.That(options.Exclusive, Is.False);
            Assert.That(options.AutoDelete, Is.False);
            Assert.That(options.Durable, Is.True);
            Assert.That(options.Args, Is.Empty);
            Assert.That(options.Bindings, Is.Empty);
        }
    }

    [TestFixture]
    public class ProvisioningBindingOptionsTests
    {
        [Test]
        public void Defaults_NoRoutingKeyEmptyArgs()
        {
            var options = new ProvisioningBindingOptions { ExchangeName = "ex" };

            Assert.That(options.ExchangeName, Is.EqualTo("ex"));
            Assert.That(options.RoutingKey, Is.Null);
            Assert.That(options.Args, Is.Empty);
        }
    }
}
