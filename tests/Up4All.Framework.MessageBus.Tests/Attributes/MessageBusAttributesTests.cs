using Up4All.Framework.MessageBus.Abstractions.Attributes;

using Up4All.Framework.MessageBus.Tests.Support;

namespace Up4All.Framework.MessageBus.Tests.Attributes
{
    [TestFixture]
    public class MessageBusAttributesTests
    {
        [Test]
        public void MessageBusPayloadAttribute_ExposesTarget()
        {
            var attribute = new MessageBusPayloadAttribute("my-target");

            Assert.That(attribute.Target, Is.EqualTo("my-target"));
        }

        [Test]
        public void MessageBusRoutingKeyAttribute_ExposesRoutingKey()
        {
            var attribute = new MessageBusRoutingKeyAttribute("my-key");

            Assert.That(attribute.RoutingKey, Is.EqualTo("my-key"));
        }

        [Test]
        public void MessageBusUserPropertyAttribute_ExposesKey()
        {
            var attribute = new MessageBusUserPropertyAttribute("my-prop");

            Assert.That(attribute.Key, Is.EqualTo("my-prop"));
        }

        [Test]
        public void MessageBusAdditionalUserPropertyAttribute_ExposesKeyAndValue()
        {
            var attribute = new MessageBusAdditionalUserPropertyAttribute("k", 10);

            Assert.That(attribute.Key, Is.EqualTo("k"));
            Assert.That(attribute.Value, Is.EqualTo(10));
        }

        [Test]
        public void Attributes_AreDeclaredOnSampleModel()
        {
            var type = typeof(SampleModel);

            var payload = type.GetCustomAttributes(typeof(MessageBusPayloadAttribute), false);
            var routingKey = type.GetCustomAttributes(typeof(MessageBusRoutingKeyAttribute), false);
            var additional = type.GetCustomAttributes(typeof(MessageBusAdditionalUserPropertyAttribute), false);

            Assert.That(payload, Has.Length.EqualTo(1));
            Assert.That(routingKey, Has.Length.EqualTo(1));
            Assert.That(additional, Has.Length.EqualTo(2));

            var codeProperty = type.GetProperty(nameof(SampleModel.Code))!;
            var userProperty = codeProperty.GetCustomAttributes(typeof(MessageBusUserPropertyAttribute), false);
            Assert.That(userProperty, Has.Length.EqualTo(1));
        }
    }
}
