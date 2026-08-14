using Up4All.Framework.MessageBus.RabbitMQ.Consts;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests.Consts
{
    [TestFixture]
    public class RabbitMQConstsTests
    {
        [Test]
        public void DefaultQueueDeclareOptions_ReturnsClassicDurableQueueWithNoArgs()
        {
            var options = RabbitMQConsts.DefaultQueueDeclareOptions;

            Assert.That(options.Type, Is.EqualTo(QueueType.Classic));
            Assert.That(options.Durable, Is.True);
            Assert.That(options.Exclusive, Is.False);
            Assert.That(options.AutoDelete, Is.False);
            Assert.That(options.Args, Is.Empty);
            Assert.That(options.Bindings, Is.Empty);
        }

        [Test]
        public void DefaultQueueDeclareOptions_ReturnsNewInstanceEachCall()
        {
            var first = RabbitMQConsts.DefaultQueueDeclareOptions;
            var second = RabbitMQConsts.DefaultQueueDeclareOptions;

            Assert.That(first, Is.Not.SameAs(second));
        }

        [Test]
        public void DefaultStreamDeclareOptions_ReturnsStreamTypeWithStreamArg()
        {
            var options = RabbitMQConsts.DefaultStreamDeclareOptions;

            Assert.That(options.Type, Is.EqualTo(QueueType.Stream));
            Assert.That(options.Args["x-stream-type"], Is.EqualTo("stream"));
        }

        [Test]
        public void DefaultExchangeDeclareOptions_ReturnsDurableExchangeWithNoArgs()
        {
            var options = RabbitMQConsts.DefaultExchangeDeclareOptions;

            Assert.That(options.Durable, Is.True);
            Assert.That(options.AutoDelete, Is.False);
            Assert.That(options.Args, Is.Empty);
        }

        [Test]
        public void ToQueueDeclare_NullProvisioningOptions_ReturnsNull()
        {
            ProvisioningOptions? opts = null;

            var result = opts.ToQueueDeclare();

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ToQueueDeclare_ValidProvisioningOptions_MapsProperties()
        {
            var opts = new ProvisioningOptions
            {
                Exclusive = true,
                Durable = false,
                AutoDelete = true,
            };
            opts.Args.Add("k", "v");

            var result = opts.ToQueueDeclare();

            Assert.That(result.Exclusive, Is.True);
            Assert.That(result.Durable, Is.False);
            Assert.That(result.AutoDelete, Is.True);
            Assert.That(result.Args["k"], Is.EqualTo("v"));
        }

        [Test]
        public void ToStreamDeclare_NullProvisioningOptions_ReturnsNull()
        {
            ProvisioningOptions? opts = null;

            var result = opts.ToStreamDeclare();

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ToStreamDeclare_ValidProvisioningOptions_MapsProperties()
        {
            var opts = new ProvisioningOptions { Durable = false };

            var result = opts.ToStreamDeclare();

            Assert.That(result.Durable, Is.False);
        }

        [Test]
        public void ToExchangeDeclare_NullProvisioningOptions_ReturnsNull()
        {
            ProvisioningOptions? opts = null;

            var result = opts.ToExchangeDeclare();

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ToExchangeDeclare_ValidProvisioningOptions_MapsProperties()
        {
            var opts = new ProvisioningOptions { AutoDelete = true };

            var result = opts.ToExchangeDeclare();

            Assert.That(result.AutoDelete, Is.True);
        }
    }
}
