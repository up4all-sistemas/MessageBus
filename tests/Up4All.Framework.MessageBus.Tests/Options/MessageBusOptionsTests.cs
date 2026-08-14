using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.Tests.Options
{
    [TestFixture]
    public class MessageBusOptionsTests
    {
        [Test]
        public void Constructor_DefaultsConnectionAttemptsToOne()
        {
            var options = new MessageBusOptions();

            Assert.That(options.ConnectionAttempts, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_NullableStringProperties_DefaultToNull()
        {
            var options = new MessageBusOptions();

            Assert.That(options.ConnectionString, Is.Null);
            Assert.That(options.QueueName, Is.Null);
            Assert.That(options.TopicName, Is.Null);
            Assert.That(options.SubscriptionName, Is.Null);
            Assert.That(options.StreamName, Is.Null);
        }

        [Test]
        public void Properties_AreSettable()
        {
            var options = new MessageBusOptions
            {
                ConnectionString = "conn",
                QueueName = "queue",
                TopicName = "topic",
                SubscriptionName = "sub",
                StreamName = "stream",
                ConnectionAttempts = 5
            };

            Assert.That(options.ConnectionString, Is.EqualTo("conn"));
            Assert.That(options.QueueName, Is.EqualTo("queue"));
            Assert.That(options.TopicName, Is.EqualTo("topic"));
            Assert.That(options.SubscriptionName, Is.EqualTo("sub"));
            Assert.That(options.StreamName, Is.EqualTo("stream"));
            Assert.That(options.ConnectionAttempts, Is.EqualTo(5));
        }
    }
}
