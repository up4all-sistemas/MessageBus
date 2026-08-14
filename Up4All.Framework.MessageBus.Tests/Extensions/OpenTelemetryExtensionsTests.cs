using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Tests.Extensions
{
    [TestFixture]
    public class OpenTelemetryExtensionsTests
    {
        private ActivityListener _listener = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // The library relies on Propagators.DefaultTextMapPropagator, which is a
            // no-op until a real propagator is configured (normally done by the hosting
            // OpenTelemetry SDK setup). Configure a real W3C propagator so injection and
            // extraction can be verified end-to-end.
            Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(
                [new TraceContextPropagator(), new BaggagePropagator()]));

            _listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
                SampleUsingParentId = (ref ActivityCreationOptions<string> options) => ActivitySamplingResult.AllData
            };
            ActivitySource.AddActivityListener(_listener);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _listener.Dispose();
        }

        [Test]
        public void CreateActivitySource_ReturnsSourceNamedAfterOwningAssembly()
        {
            var source = OpenTelemetryExtensions.CreateActivitySource<OpenTelemetryExtensionsTests>();

            Assert.That(source.Name, Is.EqualTo(typeof(OpenTelemetryExtensionsTests).Assembly.GetName().Name));
        }

        [Test]
        public void ProcessOpenTelemetryActivity_WithListener_ReturnsActivity()
        {
            var source = OpenTelemetryExtensions.CreateActivitySource<OpenTelemetryExtensionsTests>();

            using var activity = source.ProcessOpenTelemetryActivity("test-activity", ActivityKind.Producer);

            Assert.That(activity, Is.Not.Null);
            Assert.That(activity!.OperationName, Is.EqualTo("test-activity"));
            Assert.That(activity.Kind, Is.EqualTo(ActivityKind.Producer));
        }

        [Test]
        public void InjectPropagationContext_NullActivity_DoesNothing()
        {
            Activity? activity = null;
            var props = new Dictionary<string, object>();

            Assert.DoesNotThrow(() => activity.InjectPropagationContext(props));
            Assert.That(props, Is.Empty);
        }

        [Test]
        public void InjectPropagationContext_WithActivity_AddsTraceParentHeader()
        {
            var source = OpenTelemetryExtensions.CreateActivitySource<OpenTelemetryExtensionsTests>();
            using var activity = source.ProcessOpenTelemetryActivity("inject-test", ActivityKind.Producer);
            var props = new Dictionary<string, object>();

            activity.InjectPropagationContext(props);

            Assert.That(props.ContainsKey("traceparent"), Is.True);
            Assert.That((string)props["traceparent"], Does.Contain(activity!.TraceId.ToString()));
        }

        [Test]
        public void InjectPropagationContext_ExistingKey_OverwritesValue()
        {
            var source = OpenTelemetryExtensions.CreateActivitySource<OpenTelemetryExtensionsTests>();
            using var activity = source.ProcessOpenTelemetryActivity("inject-overwrite", ActivityKind.Producer);
            var props = new Dictionary<string, object> { { "traceparent", "stale-value" } };

            activity.InjectPropagationContext(props);

            Assert.That((string)props["traceparent"], Does.Contain(activity!.TraceId.ToString()));
        }

        [Test]
        public void GetParentPropagationContext_ByteArrayHeaderValue_ExtractsTraceId()
        {
            var traceId = ActivityTraceId.CreateRandom();
            var spanId = ActivitySpanId.CreateRandom();
            var traceparent = $"00-{traceId}-{spanId}-01";
            var props = new List<KeyValuePair<string, object>>
            {
                new("traceparent", Encoding.UTF8.GetBytes(traceparent))
            };

            var context = OpenTelemetryExtensions.GetParentPropagationContext(props);

            Assert.That(context.ActivityContext.TraceId, Is.EqualTo(traceId));
        }

        [Test]
        public void GetParentPropagationContext_NonStringNonByteArrayHeaderValue_UsesToString()
        {
            var traceId = ActivityTraceId.CreateRandom();
            var spanId = ActivitySpanId.CreateRandom();
            var traceparent = new ToStringOnlyValue($"00-{traceId}-{spanId}-01");
            var props = new List<KeyValuePair<string, object>>
            {
                new("traceparent", traceparent)
            };

            var context = OpenTelemetryExtensions.GetParentPropagationContext(props);

            Assert.That(context.ActivityContext.TraceId, Is.EqualTo(traceId));
        }

        private sealed class ToStringOnlyValue(string value)
        {
            public override string ToString() => value;
        }

        [Test]
        public void GetParentPropagationContext_NullProps_ReturnsDefault()
        {
            var context = OpenTelemetryExtensions.GetParentPropagationContext(null!);

            Assert.That(context.ActivityContext.TraceId, Is.EqualTo(default(ActivityTraceId)));
        }

        [Test]
        public void GetParentPropagationContext_RoundTrip_ExtractsSameTraceId()
        {
            var source = OpenTelemetryExtensions.CreateActivitySource<OpenTelemetryExtensionsTests>();
            using var activity = source.ProcessOpenTelemetryActivity("parent-activity", ActivityKind.Producer);
            var props = new Dictionary<string, object>();
            activity.InjectPropagationContext(props);

            var kvps = new List<KeyValuePair<string, object>>();
            foreach (var kvp in props)
                kvps.Add(kvp);

            var context = OpenTelemetryExtensions.GetParentPropagationContext(kvps);

            Assert.That(context.ActivityContext.TraceId, Is.EqualTo(activity!.TraceId));
        }

        [Test]
        public void CreateActivity_WithPropagatedHeaders_CreatesChildWithSameTraceId()
        {
            var source = OpenTelemetryExtensions.CreateActivitySource<OpenTelemetryExtensionsTests>();
            using var parent = source.ProcessOpenTelemetryActivity("parent-for-child", ActivityKind.Producer);
            var props = new Dictionary<string, object>();
            parent.InjectPropagationContext(props);

            using var child = source.CreateActivity(props, "child-activity", ActivityKind.Consumer);

            Assert.That(child, Is.Not.Null);
            Assert.That(child!.TraceId, Is.EqualTo(parent!.TraceId));
        }

        [Test]
        public void CreateActivity_Generic_CreatesActivityUsingSourceType()
        {
            var props = new Dictionary<string, object>();

            using var activity = props.CreateActivity<OpenTelemetryExtensionsTests>("generic-activity", ActivityKind.Consumer);

            Assert.That(activity, Is.Not.Null);
            Assert.That(activity!.OperationName, Is.EqualTo("generic-activity"));
        }

        [Test]
        public void AddTagsToActivity_NullActivity_DoesNotThrow()
        {
            Activity? activity = null;
            var message = new MessageBusMessage();
            message.AddBody("body");

            Assert.DoesNotThrow(() => activity.AddTagsToActivity("rabbitmq", message, "entity", "id"));
        }

        [Test]
        public void AddTagsToActivity_SetsExpectedTags()
        {
            var source = OpenTelemetryExtensions.CreateActivitySource<OpenTelemetryExtensionsTests>();
            using var activity = source.ProcessOpenTelemetryActivity("tags-activity", ActivityKind.Producer);
            var message = new MessageBusMessage();
            message.AddBody("hello");

            activity.AddTagsToActivity("rabbitmq", message, "my-entity", "msg-id", operationType: "send");

            Assert.That(activity!.GetTagItem("messaging.system"), Is.EqualTo("rabbitmq"));
            Assert.That(activity.GetTagItem("messaging.destination.name"), Is.EqualTo("my-entity"));
            Assert.That(activity.GetTagItem("messaging.operation.type"), Is.EqualTo("send"));
            Assert.That(activity.GetTagItem("messaging.message.id"), Is.EqualTo("msg-id"));
        }

        [Test]
        public void AddTagsToActivity_DoesNotIncludeMessageBodyAsATag()
        {
            // Message bodies can carry PII/secrets and traces are typically retained longer
            // and access-controlled more loosely than logs, so the body must never end up as
            // a span attribute (see the OpenTelemetry messaging semantic conventions).
            var source = OpenTelemetryExtensions.CreateActivitySource<OpenTelemetryExtensionsTests>();
            using var activity = source.ProcessOpenTelemetryActivity("tags-no-body", ActivityKind.Producer);
            var message = new MessageBusMessage();
            message.AddBody("sensitive-payload");

            activity.AddTagsToActivity("rabbitmq", message, "my-entity", "msg-id");

            Assert.That(activity!.GetTagItem("body"), Is.Null);
        }

        [Test]
        public void AddTagsToActivity_WithCorrelationId_SetsConversationIdTag()
        {
            var source = OpenTelemetryExtensions.CreateActivitySource<OpenTelemetryExtensionsTests>();
            using var activity = source.ProcessOpenTelemetryActivity("tags-correlation", ActivityKind.Producer);
            var message = new MessageBusMessage();
            message.AddBody("hello");
            var correlationId = System.Guid.NewGuid();
            message.SetCorrelationId(correlationId);

            activity.AddTagsToActivity("rabbitmq", message, "entity", null);

            Assert.That(activity!.GetTagItem("messaging.message.conversation_id"), Is.EqualTo(correlationId));
        }

        [Test]
        public void AddTagsToActivity_WithAdditionalTags_SetsThem()
        {
            var source = OpenTelemetryExtensions.CreateActivitySource<OpenTelemetryExtensionsTests>();
            using var activity = source.ProcessOpenTelemetryActivity("tags-additional", ActivityKind.Producer);
            var message = new MessageBusMessage();
            message.AddBody("hello");
            var additional = new Dictionary<string, object> { { "custom.tag", "custom-value" } };

            activity.AddTagsToActivity("rabbitmq", message, "entity", null, additionalTags: additional);

            Assert.That(activity!.GetTagItem("custom.tag"), Is.EqualTo("custom-value"));
        }
    }
}
