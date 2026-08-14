using System;
using System.Text;
using System.Text.Json;

using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;

using Up4All.Framework.MessageBus.Tests.Support;

namespace Up4All.Framework.MessageBus.Tests.Extensions
{
    [TestFixture]
    public class MessageBusExtensionsTests
    {
        [Test]
        public void CreateMessagebusMessage_NullModel_Throws()
        {
            PlainModel? model = null;

            Assert.Throws<ArgumentNullException>(() => model!.CreateMessagebusMessage());
        }

        [Test]
        public void CreateMessagebusMessage_WithAttributes_PopulatesUserPropertiesAndBody()
        {
            var model = new SampleModel { Name = "n1", Code = 7 };

            var message = model.CreateMessagebusMessage();

            Assert.That(message.UserProperties["target"], Is.EqualTo("my-target"));
            Assert.That(message.UserProperties["routing-key"], Is.EqualTo("my-routing-key"));
            Assert.That(message.UserProperties["extra-key"], Is.EqualTo("extra-value"));
            Assert.That(message.UserProperties["extra-key-2"], Is.EqualTo(42));
            Assert.That(message.UserProperties["custom-prop"], Is.EqualTo(7));

            var json = Encoding.UTF8.GetString(message.Body);
            Assert.That(json, Does.Contain("\"name\":\"n1\""));
        }

        [Test]
        public void CreateMessagebusMessage_DoesNotMarkBodyAsJson()
        {
            // Current behavior: CreateMessagebusMessage calls AddBody(BinaryData) without
            // requesting the isJsonData flag, so IsJson stays false even though the body is JSON.
            var model = new SampleModel { Name = "n1", Code = 1 };

            var message = model.CreateMessagebusMessage();

            Assert.That(message.IsJson, Is.False);
        }

        [Test]
        public void CreateMessagebusMessage_WithoutAttributes_DoesNotAddTargetOrRoutingKey()
        {
            var model = new PlainModel { Value = "v" };

            var message = model.CreateMessagebusMessage();

            Assert.That(message.UserProperties.ContainsKey("target"), Is.False);
            Assert.That(message.UserProperties.ContainsKey("routing-key"), Is.False);
        }

        [Test]
        public void AddRoutingKey_ContainsRoutingKey_GetRoutingKey_RoundTrip()
        {
            var message = new MessageBusMessage();

            Assert.That(message.ContainsRoutingKey(), Is.False);

            message.AddRoutingKey("rk-1");

            Assert.That(message.ContainsRoutingKey(), Is.True);
            Assert.That(message.GetRoutingKey(), Is.EqualTo("rk-1"));
        }

        [Test]
        public void AddRoutingKey_CalledTwice_ReplacesValue()
        {
            var message = new MessageBusMessage();

            message.AddRoutingKey("rk-1");
            message.AddRoutingKey("rk-2");

            Assert.That(message.GetRoutingKey(), Is.EqualTo("rk-2"));
        }

        [Test]
        public void GetUserPropertyAsString_StringValue_ReturnsIt()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", "v");

            Assert.That(message.GetUserPropertyAsString("k"), Is.EqualTo("v"));
        }

        [Test]
        public void GetUserPropertyAsString_BytesValue_DecodesUtf8()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", Encoding.UTF8.GetBytes("b"));

            Assert.That(message.GetUserPropertyAsString("k"), Is.EqualTo("b"));
        }

        [Test]
        public void GetUserPropertyAsString_OtherValue_UsesToString()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", 123);

            Assert.That(message.GetUserPropertyAsString("k"), Is.EqualTo("123"));
        }

        [Test]
        public void GetUserPropertyAsString_MissingKey_ReturnsDefault()
        {
            var message = new MessageBusMessage();

            Assert.That(message.GetUserPropertyAsString("missing", "fallback"), Is.EqualTo("fallback"));
        }

        [Test]
        public void GetUserPropertyAsString_NullValue_ReturnsDefault()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", null!);

            Assert.That(message.GetUserPropertyAsString("k", "fallback"), Is.EqualTo("fallback"));
        }

        [Test]
        public void TryGetUserPropertyAsString_StringValue_ReturnsTrue()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", "v");

            var result = message.TryGetUserPropertyAsString("k", out var value);

            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo("v"));
        }

        [Test]
        public void TryGetUserPropertyAsString_BytesValue_ReturnsTrue()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", Encoding.UTF8.GetBytes("v"));

            var result = message.TryGetUserPropertyAsString("k", out var value);

            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo("v"));
        }

        [Test]
        public void TryGetUserPropertyAsString_UnsupportedType_ReturnsFalse()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", 123);

            var result = message.TryGetUserPropertyAsString("k", out var value);

            Assert.That(result, Is.False);
            Assert.That(value, Is.Null);
        }

        [Test]
        public void TryGetUserPropertyAsString_MissingKey_ReturnsFalse()
        {
            var message = new MessageBusMessage();

            Assert.That(message.TryGetUserPropertyAsString("missing", out _), Is.False);
        }

        [Test]
        public void TryGetUserPropertyAsInt32_NumericValue_ReturnsTrue()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", 42);

            var result = message.TryGetUserPropertyAsInt32("k", out var value);

            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo(42));
        }

        [Test]
        public void TryGetUserPropertyAsInt32_BytesValue_ReturnsFalse()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", new byte[] { 1 });

            Assert.That(message.TryGetUserPropertyAsInt32("k", out _), Is.False);
        }

        [Test]
        public void TryGetUserPropertyAsInt32_MissingKey_ReturnsFalse()
        {
            var message = new MessageBusMessage();

            Assert.That(message.TryGetUserPropertyAsInt32("missing", out _), Is.False);
        }

        [Test]
        public void TryGetUserPropertyAsInt64_NumericValue_ReturnsTrue()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", 42L);

            var result = message.TryGetUserPropertyAsInt64("k", out var value);

            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo(42L));
        }

        [Test]
        public void TryGetUserPropertyAsInt64_BytesValue_ReturnsFalse()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", new byte[] { 1 });

            Assert.That(message.TryGetUserPropertyAsInt64("k", out _), Is.False);
        }

        [Test]
        public void TryGetUserPropertyAsInt64_MissingKey_ReturnsFalse()
        {
            var message = new MessageBusMessage();

            Assert.That(message.TryGetUserPropertyAsInt64("missing", out _), Is.False);
        }

        [Test]
        public void TryGetUserPropertyAsDecimal_NumericValue_ReturnsTrue()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", 42.5m);

            var result = message.TryGetUserPropertyAsDecimal("k", out var value);

            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo(42.5m));
        }

        [Test]
        public void TryGetUserPropertyAsDecimal_BytesValue_ReturnsFalse()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", new byte[] { 1 });

            Assert.That(message.TryGetUserPropertyAsDecimal("k", out _), Is.False);
        }

        [Test]
        public void TryGetUserPropertyAsDateTime_MissingKey_ReturnsFalse()
        {
            // Current behavior: this method always returns false, regardless of the outcome
            // of the internal branching (see TryGetUserPropertyAsDateTime implementation).
            var message = new MessageBusMessage();

            Assert.That(message.TryGetUserPropertyAsDateTime("missing", out _), Is.False);
        }

        [Test]
        public void TryGetUserPropertyAsDateTime_NonByteArrayValue_ReturnsFalseWithoutThrowing()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", DateTime.UtcNow);

            var result = message.TryGetUserPropertyAsDateTime("k", out var value);

            Assert.That(result, Is.False);
            Assert.That(value, Is.EqualTo(default(DateTime)));
        }

        [Test]
        public void TryGetUserPropertyAsDateTime_ByteArrayValue_ThrowsInvalidCast()
        {
            // Current behavior: Convert.ToDateTime(byte[]) throws because byte[] does not
            // implement IConvertible.
            var message = new MessageBusMessage();
            message.AddUserProperty("k", new byte[] { 1, 2, 3 });

            Assert.Throws<InvalidCastException>(() => message.TryGetUserPropertyAsDateTime("k", out _));
        }

        [Test]
        public void TryGetUserPropertyAsDecimal_MissingKey_ReturnsFalse()
        {
            var message = new MessageBusMessage();

            Assert.That(message.TryGetUserPropertyAsDecimal("missing", out _), Is.False);
        }

        [Test]
        public void TryGetUserPropertyAsDateTime_WithFormatProvider_MissingKey_ReturnsFalse()
        {
            var message = new MessageBusMessage();

            Assert.That(message.TryGetUserPropertyAsDateTime(System.Globalization.CultureInfo.InvariantCulture, "missing", out _), Is.False);
        }

        [Test]
        public void TryGetUserPropertyAsDateTime_WithFormatProvider_NonByteArrayValue_ReturnsFalse()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", DateTime.UtcNow);

            var result = message.TryGetUserPropertyAsDateTime(System.Globalization.CultureInfo.InvariantCulture, "k", out var value);

            Assert.That(result, Is.False);
            Assert.That(value, Is.EqualTo(default(DateTime)));
        }

        [Test]
        public void TryGetUserPropertyAsDateTime_WithFormatProvider_ByteArrayValue_ThrowsInvalidCast()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", new byte[] { 1, 2, 3 });

            Assert.Throws<InvalidCastException>(() =>
                message.TryGetUserPropertyAsDateTime(System.Globalization.CultureInfo.InvariantCulture, "k", out _));
        }

        [Test]
        public void TryGetUserPropertyAsObject_ByteArrayValue_ReturnsFalse()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", new byte[] { 1 });

            Assert.That(message.TryGetUserPropertyAsObject("k", out _), Is.False);
        }

        [Test]
        public void TryGetUserPropertyAsObject_OtherValue_ReturnsTrue()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", "v");

            var result = message.TryGetUserPropertyAsObject("k", out var value);

            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo("v"));
        }

        [Test]
        public void TryGetUserPropertyAsObject_MissingKey_ReturnsFalse()
        {
            var message = new MessageBusMessage();

            Assert.That(message.TryGetUserPropertyAsObject("missing", out _), Is.False);
        }

        [Test]
        public void TryGetUserPropertyAs_ValidJson_ReturnsDeserializedObject()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", JsonSerializer.Serialize(new IdPayload { Id = 3, Name = "three" }));

            var result = message.TryGetUserPropertyAs<IdPayload>("k", out var value);

            Assert.That(result, Is.True);
            Assert.That(value!.Id, Is.EqualTo(3));
        }

        [Test]
        public void TryGetUserPropertyAs_InvalidJson_ReturnsFalse()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", "not-json");

            var result = message.TryGetUserPropertyAs<IdPayload>("k", out var value);

            Assert.That(result, Is.False);
            Assert.That(value, Is.Null);
        }

        [Test]
        public void TryGetUserPropertyAs_MissingKey_ReturnsFalse()
        {
            var message = new MessageBusMessage();

            Assert.That(message.TryGetUserPropertyAs<IdPayload>("missing", out _), Is.False);
        }

        [Test]
        public void TryGetUserPropertyValue_ByteArrayValue_ReturnsTrue()
        {
            var message = new MessageBusMessage();
            var bytes = new byte[] { 1, 2 };
            message.AddUserProperty("k", bytes);

            var result = message.TryGetUserPropertyValue("k", out var value);

            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo(bytes));
        }

        [Test]
        public void TryGetUserPropertyValue_OtherValue_ReturnsTrue()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", "v");

            var result = message.TryGetUserPropertyValue("k", out var value);

            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo("v"));
        }

        [Test]
        public void TryGetUserPropertyValue_MissingKey_ReturnsFalse()
        {
            var message = new MessageBusMessage();

            Assert.That(message.TryGetUserPropertyValue("missing", out var value), Is.False);
            Assert.That(value, Is.Null);
        }

        [Test]
        public void TryGetUserProperty_Struct_Found_ReturnsTrue()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", 55);

            var result = message.TryGetUserProperty<int>("k", out var value);

            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo(55));
        }

        [Test]
        public void TryGetUserProperty_Struct_MissingKey_ReturnsFalse()
        {
            var message = new MessageBusMessage();

            var result = message.TryGetUserProperty<int>("missing", out var value);

            Assert.That(result, Is.False);
            Assert.That(value, Is.EqualTo(0));
        }

        [Test]
        public void SetCorrelationId_And_GetCorrelationId_RoundTrip()
        {
            var message = new MessageBusMessage();
            var correlationId = Guid.NewGuid();

            message.SetCorrelationId(correlationId);

            Assert.That(message.GetCorrelationId(), Is.EqualTo(correlationId));
        }

        [Test]
        public void GetCorrelationId_WhenNotSet_ReturnsNull()
        {
            var message = new MessageBusMessage();

            Assert.That(message.GetCorrelationId(), Is.Null);
        }
    }
}
