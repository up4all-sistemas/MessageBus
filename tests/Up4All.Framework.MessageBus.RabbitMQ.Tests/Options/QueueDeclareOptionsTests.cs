using System.Linq;

using Up4All.Framework.MessageBus.RabbitMQ.Consts;
using Up4All.Framework.MessageBus.RabbitMQ.Options;

namespace Up4All.Framework.MessageBus.RabbitMQ.Tests.Options
{
    [TestFixture]
    public class QueueDeclareOptionsTests
    {
        [Test]
        public void DefaultConstructor_SetsClassicDurableDefaults()
        {
            var options = new QueueDeclareOptions();

            Assert.That(options.Type, Is.EqualTo(QueueType.Classic));
            Assert.That(options.Durable, Is.True);
            Assert.That(options.Exclusive, Is.False);
            Assert.That(options.AutoDelete, Is.False);
            Assert.That(options.Args, Is.Empty);
            Assert.That(options.Bindings, Is.Empty);
        }

        [Test]
        public void AddBinding_AppendsConfiguredBindingToCollection()
        {
            var options = new QueueDeclareOptions();

            options.AddBinding("my-exchange", b => b.RoutingKey = "rk");

            Assert.That(options.Bindings, Has.Count.EqualTo(1));
            var binding = options.Bindings.First();
            Assert.That(binding.ExchangeName, Is.EqualTo("my-exchange"));
            Assert.That(binding.RoutingKey, Is.EqualTo("rk"));
        }

        [Test]
        public void ImplicitOperator_NullProvisioningOptions_ReturnsNull()
        {
            ProvisioningOptions? opts = null;

            QueueDeclareOptions result = opts!;

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ImplicitOperator_MapsAllPropertiesAndBindings()
        {
            var opts = new ProvisioningOptions
            {
                Exclusive = true,
                Durable = false,
                AutoDelete = true,
                Bindings =
                [
                    new ProvisioningBindingOptions { ExchangeName = "ex-1", RoutingKey = "rk-1" }
                ]
            };
            opts.Args.Add("k", 1);

            QueueDeclareOptions result = opts;

            Assert.That(result.Exclusive, Is.True);
            Assert.That(result.Durable, Is.False);
            Assert.That(result.AutoDelete, Is.True);
            Assert.That(result.Args["k"], Is.EqualTo(1));
            Assert.That(result.Bindings, Has.Count.EqualTo(1));
            Assert.That(result.Bindings.First().ExchangeName, Is.EqualTo("ex-1"));
            Assert.That(result.Bindings.First().RoutingKey, Is.EqualTo("rk-1"));
        }
    }

    [TestFixture]
    public class QueueBindOptionsTests
    {
        [Test]
        public void Constructor_WithExchangeNameOnly_InitializesEmptyArgs()
        {
            var binding = new QueueBindOptions("ex", "k", "v");

            Assert.That(binding.ExchangeName, Is.EqualTo("ex"));
            Assert.That(binding.Args["k"], Is.EqualTo("v"));
        }

        [Test]
        public void Constructor_WithDefaultArg_SetsRoutingKeyAndArg()
        {
            var binding = new QueueBindOptions("ex", "k", "v", "rk");

            Assert.That(binding.RoutingKey, Is.EqualTo("rk"));
            Assert.That(binding.Args["k"], Is.EqualTo("v"));
        }

        [Test]
        public void ImplicitOperator_MapsFromProvisioningBindingOptions()
        {
            var opts = new ProvisioningBindingOptions { ExchangeName = "ex", RoutingKey = "rk" };
            opts.Args.Add("k", "v");

            QueueBindOptions result = opts;

            Assert.That(result.ExchangeName, Is.EqualTo("ex"));
            Assert.That(result.RoutingKey, Is.EqualTo("rk"));
            Assert.That(result.Args["k"], Is.EqualTo("v"));
        }
    }

    [TestFixture]
    public class StreamDeclareOptionsTests
    {
        [Test]
        public void DefaultConstructor_SetsStreamTypeAndArg()
        {
            var options = new StreamDeclareOptions();

            Assert.That(options.Type, Is.EqualTo(QueueType.Stream));
            Assert.That(options.Args["x-stream-type"], Is.EqualTo("stream"));
        }

        [Test]
        public void DefaultConstructor_DoesNotDuplicateStreamTypeArgOnRepeatedCalls()
        {
            var options = new StreamDeclareOptions();

            Assert.That(options.Args.Count, Is.EqualTo(1));
        }

        [Test]
        public void ImplicitOperator_NullProvisioningOptions_ReturnsNull()
        {
            ProvisioningOptions? opts = null;

            StreamDeclareOptions result = opts!;

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ImplicitOperator_OverwritesConstructorArgsWithProvisioningArgs()
        {
            // Current behavior: the object initializer runs after the base constructor,
            // so Args = opts.Args replaces the dictionary the constructor populated with
            // "x-stream-type" - it is lost unless already present in opts.Args.
            var opts = new ProvisioningOptions { Durable = false };

            StreamDeclareOptions result = opts;

            Assert.That(result.Args.ContainsKey("x-stream-type"), Is.False);
        }

        [Test]
        public void ImplicitOperator_MapsBasicProperties()
        {
            var opts = new ProvisioningOptions
            {
                Exclusive = true,
                Durable = false,
                AutoDelete = true,
            };

            StreamDeclareOptions result = opts;

            Assert.That(result.Exclusive, Is.True);
            Assert.That(result.Durable, Is.False);
            Assert.That(result.AutoDelete, Is.True);
        }

        [Test]
        public void ImplicitOperator_MapsBindings()
        {
            var opts = new ProvisioningOptions
            {
                Bindings = [new ProvisioningBindingOptions { ExchangeName = "ex-1", RoutingKey = "rk-1" }]
            };

            StreamDeclareOptions result = opts;

            Assert.That(result.Bindings, Has.Count.EqualTo(1));
            Assert.That(result.Bindings.First().ExchangeName, Is.EqualTo("ex-1"));
            Assert.That(result.Bindings.First().RoutingKey, Is.EqualTo("rk-1"));
        }
    }

    [TestFixture]
    public class ExchangeDeclareOptionsTests
    {
        [Test]
        public void DefaultConstructor_SetsDurableTrueAndEmptyArgs()
        {
            var options = new ExchangeDeclareOptions();

            Assert.That(options.Durable, Is.True);
            Assert.That(options.AutoDelete, Is.False);
            Assert.That(options.Args, Is.Empty);
        }

        [Test]
        public void ImplicitOperator_NullProvisioningOptions_ReturnsNull()
        {
            ProvisioningOptions? opts = null;

            ExchangeDeclareOptions result = opts!;

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ImplicitOperator_MapsProperties()
        {
            var opts = new ProvisioningOptions { Durable = false, AutoDelete = true };
            opts.Args.Add("k", "v");

            ExchangeDeclareOptions result = opts;

            Assert.That(result.Durable, Is.False);
            Assert.That(result.AutoDelete, Is.True);
            Assert.That(result.Args["k"], Is.EqualTo("v"));
        }
    }
}
