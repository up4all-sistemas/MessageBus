using System.Text;

using Up4All.Framework.MessageBus.Abstractions.Messages;

using Up4All.Framework.MessageBus.Tests.Support;

namespace Up4All.Framework.MessageBus.Tests.Messages
{
    [TestFixture]
    public class ReceivedMessageTests
    {
        [Test]
        public void GetBody_ReturnsUtf8String()
        {
            var message = new ReceivedMessage();
            message.AddBody("hello world");

            Assert.That(message.GetBody(), Is.EqualTo("hello world"));
        }

        [Test]
        public void GetBodyGeneric_DeserializesJson()
        {
            var message = new ReceivedMessage();
            message.AddBody("{\"id\":10,\"name\":\"ten\"}");

            var result = message.GetBody<IdPayload>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(10));
            Assert.That(result.Name, Is.EqualTo("ten"));
        }

        [Test]
        public void GetBodyGeneric_EmptyBody_ReturnsDefault()
        {
            var message = new ReceivedMessage();
            message.AddBody("null");

            var result = message.GetBody<IdPayload>();

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetUserPropertyValue_ExistingKey_ReturnsValue()
        {
            var message = new ReceivedMessage();
            message.AddUserProperty("k", "v");

            Assert.That(message.GetUserPropertyValue("k"), Is.EqualTo("v"));
        }

        [Test]
        public void GetUserPropertyValue_MissingKey_ReturnsNull()
        {
            var message = new ReceivedMessage();

            Assert.That(message.GetUserPropertyValue("missing"), Is.Null);
        }

        [Test]
        public void GetUserPropertyValueAsString_StringValue_ReturnsIt()
        {
            var message = new ReceivedMessage();
            message.AddUserProperty("k", "v");

            Assert.That(message.GetUserPropertyValueAsString("k"), Is.EqualTo("v"));
        }

        [Test]
        public void GetUserPropertyValueAsString_BytesValue_DecodesUtf8()
        {
            var message = new ReceivedMessage();
            message.AddUserProperty("k", Encoding.UTF8.GetBytes("bytes-value"));

            Assert.That(message.GetUserPropertyValueAsString("k"), Is.EqualTo("bytes-value"));
        }

        [Test]
        public void GetUserPropertyValueAsString_MissingKey_ReturnsDefault()
        {
            var message = new ReceivedMessage();

            Assert.That(message.GetUserPropertyValueAsString("missing", "fallback"), Is.EqualTo("fallback"));
        }
    }
}
