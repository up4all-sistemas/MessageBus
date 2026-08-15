using System;
using System.Collections.Generic;
using System.Text;

using Up4All.Framework.MessageBus.Abstractions.Consts;
using Up4All.Framework.MessageBus.Abstractions.Messages;

using Up4All.Framework.MessageBus.Tests.Support;

namespace Up4All.Framework.MessageBus.Tests.Messages
{
    [TestFixture]
    public class MessageBusMessageTests
    {
        [Test]
        public void Constructor_Default_InitializesEmptyState()
        {
            var message = new MessageBusMessage();

            Assert.That(message.Body, Is.Empty);
            Assert.That(message.IsJson, Is.False);
            Assert.That(message.UserProperties, Is.Empty);
        }

        [Test]
        public void Constructor_WithBody_SetsBody()
        {
            var body = Encoding.UTF8.GetBytes("payload");
            var message = new MessageBusMessage(body);

            Assert.That(message.Body, Is.EqualTo(body));
            Assert.That(message.IsJson, Is.False);
        }

        [Test]
        public void AddBody_ByteArray_SetsBody()
        {
            var message = new MessageBusMessage();
            var body = new byte[] { 1, 2, 3 };

            message.AddBody(body);

            Assert.That(message.Body, Is.EqualTo(body));
        }

        [Test]
        public void AddBody_String_EncodesAsUtf8()
        {
            var message = new MessageBusMessage();

            message.AddBody("hello");

            Assert.That(Encoding.UTF8.GetString(message.Body), Is.EqualTo("hello"));
        }

        [Test]
        public void AddBody_BinaryData_SetsBodyAndIsJsonFlag()
        {
            var message = new MessageBusMessage();
            var data = BinaryData.FromString("{\"a\":1}");

            message.AddBody(data, isJsonData: true);

            Assert.That(message.Body, Is.EqualTo(data.ToArray()));
            Assert.That(message.IsJson, Is.True);
        }

        [Test]
        public void AddBody_BinaryData_DefaultsIsJsonToFalse()
        {
            var message = new MessageBusMessage();

            message.AddBody(BinaryData.FromString("plain"));

            Assert.That(message.IsJson, Is.False);
        }

        [Test]
        public void AddBody_Generic_SerializesAsJsonAndSetsIsJson()
        {
            var message = new MessageBusMessage();

            message.AddBody(new IdPayload { Id = 7, Name = "seven" });

            Assert.That(message.IsJson, Is.True);
            var json = Encoding.UTF8.GetString(message.Body);
            Assert.That(json, Does.Contain("\"id\":7"));
            Assert.That(json, Does.Contain("\"name\":\"seven\""));
        }

        [Test]
        public void AddUserProperty_KeyValuePair_AddsProperty()
        {
            var message = new MessageBusMessage();

            message.AddUserProperty(new KeyValuePair<string, object>("k", "v"));

            Assert.That(message.UserProperties["k"], Is.EqualTo("v"));
        }

        [Test]
        public void AddUserProperty_KeyValue_AddsProperty()
        {
            var message = new MessageBusMessage();

            message.AddUserProperty("k", 123);

            Assert.That(message.UserProperties["k"], Is.EqualTo(123));
        }

        [Test]
        public void AddUserProperty_ExistingKeyWithoutReplace_DoesNotOverride()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", "first");

            message.AddUserProperty("k", "second", replace: false);

            Assert.That(message.UserProperties["k"], Is.EqualTo("first"));
        }

        [Test]
        public void AddUserProperty_ExistingKeyWithReplace_Overrides()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", "first");

            message.AddUserProperty("k", "second", replace: true);

            Assert.That(message.UserProperties["k"], Is.EqualTo("second"));
        }

        [Test]
        public void AddUserProperties_AddsAllProvided()
        {
            var message = new MessageBusMessage();
            var props = new Dictionary<string, object>
            {
                { "a", 1 },
                { "b", 2 }
            };

            message.AddUserProperties(props);

            Assert.That(message.UserProperties["a"], Is.EqualTo(1));
            Assert.That(message.UserProperties["b"], Is.EqualTo(2));
        }

        [Test]
        public void RemoveUserProperty_ExistingKey_Removes()
        {
            var message = new MessageBusMessage();
            message.AddUserProperty("k", "v");

            message.RemoveUserProperty("k");

            Assert.That(message.UserProperties.ContainsKey("k"), Is.False);
        }

        [Test]
        public void RemoveUserProperty_MissingKey_DoesNotThrow()
        {
            var message = new MessageBusMessage();

            Assert.DoesNotThrow(() => message.RemoveUserProperty("missing"));
        }

        [Test]
        public void SetMessageIdFromStruct_And_GetMessageIdForStruct_RoundTrips()
        {
            var message = new MessageBusMessage();

            message.SetMessageIdFromStruct(42);

            Assert.That(message.GetMessageIdForStruct<int>(), Is.EqualTo(42));
        }

        [Test]
        public void GetMessageIdForStruct_WhenNotSet_ReturnsDefault()
        {
            var message = new MessageBusMessage();

            Assert.That(message.GetMessageIdForStruct<int>(), Is.EqualTo(0));
        }

        [Test]
        public void SetMessageId_Class_And_GetMessageIdForClass_RoundTrips()
        {
            var message = new MessageBusMessage();

            message.SetMessageId(new IdPayload { Id = 5, Name = "five" });
            var result = message.GetMessageIdForClass<IdPayload>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(5));
            Assert.That(result.Name, Is.EqualTo("five"));
        }

        [Test]
        public void SetMessageId_String_And_GetMessageIdForClass_RoundTrips()
        {
            var message = new MessageBusMessage();

            message.SetMessageId("abc-123");

            Assert.That(message.GetMessageIdForClass<string>(), Is.EqualTo("abc-123"));
        }

        [Test]
        public void GetMessageIdForClass_WhenNotSet_ReturnsNull()
        {
            var message = new MessageBusMessage();

            Assert.That(message.GetMessageIdForClass<string>(), Is.Null);
        }

        [Test]
        public void TryGetMessageIdAsString_WhenSet_ReturnsTrueAndValue()
        {
            var message = new MessageBusMessage();
            message.SetMessageId("abc-123");

            var found = message.TryGetMessageIdAsString(out var value);

            Assert.That(found, Is.True);
            Assert.That(value, Is.EqualTo("abc-123"));
        }

        [Test]
        public void TryGetMessageIdAsString_WhenNotSet_ReturnsFalse()
        {
            var message = new MessageBusMessage();

            var found = message.TryGetMessageIdAsString(out _);

            Assert.That(found, Is.False);
        }

        [Test]
        public void TryGetMessageIdAsInt32_WhenSet_ReturnsTrueAndValue()
        {
            var message = new MessageBusMessage();
            message.SetMessageId(42);

            var found = message.TryGetMessageIdAsInt32(out var value);

            Assert.That(found, Is.True);
            Assert.That(value, Is.EqualTo(42));
        }

        [Test]
        public void TryGetMessageIdAsInt32_WhenNotSet_ReturnsFalse()
        {
            var message = new MessageBusMessage();

            var found = message.TryGetMessageIdAsInt32(out var value);

            Assert.That(found, Is.False);
            Assert.That(value, Is.EqualTo(0));
        }

        [Test]
        public void TryGetMessageIdAsInt64_WhenSet_ReturnsTrueAndValue()
        {
            var message = new MessageBusMessage();
            message.SetMessageId(99L);

            var found = message.TryGetMessageIdAsInt64(out var value);

            Assert.That(found, Is.True);
            Assert.That(value, Is.EqualTo(99L));
        }

        [Test]
        public void TryGetMessageIdAsInt64_WhenNotSet_ReturnsFalse()
        {
            var message = new MessageBusMessage();

            var found = message.TryGetMessageIdAsInt64(out var value);

            Assert.That(found, Is.False);
            Assert.That(value, Is.EqualTo(0L));
        }

        [Test]
        public void TryGetMessageIdAsGuid_WhenSet_ReturnsTrueAndValue()
        {
            var message = new MessageBusMessage();
            var guid = Guid.NewGuid();
            message.SetMessageId(guid);

            var found = message.TryGetMessageIdAsGuid(out var value);

            Assert.That(found, Is.True);
            Assert.That(value, Is.EqualTo(guid));
        }

        [Test]
        public void TryGetMessageIdAsGuid_WhenNotSet_ReturnsFalse()
        {
            var message = new MessageBusMessage();

            var found = message.TryGetMessageIdAsGuid(out var value);

            Assert.That(found, Is.False);
            Assert.That(value, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void TryGetMessageIdAsGuid_WhenStoredValueIsNotAValidGuid_ReturnsFalse()
        {
            var message = new MessageBusMessage();
            message.SetMessageId("not-a-guid");

            var found = message.TryGetMessageIdAsGuid(out var value);

            Assert.That(found, Is.False);
            Assert.That(value, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void TryGetMessageIdAs_Class_WhenSet_ReturnsTrueAndValue()
        {
            var message = new MessageBusMessage();
            message.SetMessageId(new IdPayload { Id = 5, Name = "five" });

            var found = message.TryGetMessageIdAs<IdPayload>(out var value);

            Assert.That(found, Is.True);
            Assert.That(value, Is.Not.Null);
            Assert.That(value!.Id, Is.EqualTo(5));
            Assert.That(value.Name, Is.EqualTo("five"));
        }

        [Test]
        public void TryGetMessageIdAs_Class_WhenNotSet_ReturnsFalse()
        {
            var message = new MessageBusMessage();

            var found = message.TryGetMessageIdAs<IdPayload>(out var value);

            Assert.That(found, Is.False);
            Assert.That(value, Is.Null);
        }

        [Test]
        public void SetMessageId_Int_StoresValue()
        {
            var message = new MessageBusMessage();

            message.SetMessageId(99);

            Assert.That(message.UserProperties[MessageBusMessage.MessageIdkey], Is.EqualTo(99));
        }

        [Test]
        public void SetMessageId_Long_StoresValue()
        {
            var message = new MessageBusMessage();

            message.SetMessageId(99L);

            Assert.That(message.UserProperties[MessageBusMessage.MessageIdkey], Is.EqualTo(99L));
        }

        [Test]
        public void SetMessageId_Guid_StoresStringRepresentation()
        {
            var message = new MessageBusMessage();
            var guid = Guid.NewGuid();

            message.SetMessageId(guid);

            Assert.That(message.UserProperties[MessageBusMessage.MessageIdkey], Is.EqualTo(guid.ToString()));
        }

        [Test]
        public void AddTraceProperties_AddsTimestampProviderAndMessageId()
        {
            var message = new MessageBusMessage();

            message.AddTraceProperties("rabbitmq");

            Assert.That(message.UserProperties.ContainsKey(MessageBusProperties.Timestamp), Is.True);
            Assert.That(message.UserProperties[MessageBusProperties.Provider], Is.EqualTo("rabbitmq"));

            var idValue = (string)message.UserProperties[MessageBusProperties.MessageId];
            Assert.That(Guid.TryParse(idValue, out _), Is.True);

            var timestamp = (string)message.UserProperties[MessageBusProperties.Timestamp];
            Assert.That(DateTime.TryParse(timestamp, out _), Is.True);
        }
    }
}
