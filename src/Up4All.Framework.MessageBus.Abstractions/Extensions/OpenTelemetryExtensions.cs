using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Up4All.Framework.MessageBus.Abstractions.Messages;

namespace Up4All.Framework.MessageBus.Abstractions.Extensions
{
    public static class OpenTelemetryExtensions
    {
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

        public static ActivitySource CreateActivitySource<TSource>()
            where TSource : class
        {
            var assemblyName = typeof(TSource).Assembly.GetName();
            return new ActivitySource(assemblyName.Name, assemblyName.Version.ToString());
        }

        public static Activity ProcessOpenTelemetryActivity(this ActivitySource activitySource, string activityName, ActivityKind kind, ActivityContext parent = default)
        {
            var activity = activitySource.StartActivity(activityName, kind, parent);
            return activity;
        }

        public static Activity CreateActivity<TSource>(this IEnumerable<KeyValuePair<string, object>> properties, string activityName, ActivityKind kind)
            where TSource : class
        {
            var activitySource = CreateActivitySource<TSource>();
            return CreateActivity(activitySource, properties, activityName, kind);
        }

        public static Activity CreateActivity(this ActivitySource activitySource, IEnumerable<KeyValuePair<string, object>> properties, string activityName, ActivityKind kind)
        {
            var parentContext = GetParentPropagationContext(properties);
            var activity = ProcessOpenTelemetryActivity(activitySource, activityName, kind, parentContext.ActivityContext);
            return activity;
        }

        public static void InjectPropagationContext(this Activity activity, IDictionary<string, object> props)
        {
            Propagator.Inject(new PropagationContext(activity.Context, Baggage.Current), props, (x, key, value) =>
            {
                if (props.ContainsKey(key))
                    props[key] = value;
                else
                    props.Add(key, value);
            });
        }

        public static PropagationContext GetParentPropagationContext(IEnumerable<KeyValuePair<string, object>> props)
        {
            var parent = Propagator.Extract(default, props, (x, key) =>
            {
                if (x.Any(k => k.Key == key))
                {
                    var vlr = x.First(k => k.Key == key).Value;

                    if (vlr is string stringvlr)
                        return [stringvlr];
                    else
                        return [Encoding.UTF8.GetString((byte[])vlr)];
                }

                return [];
            });
            Baggage.Current = parent.Baggage;

            return parent;
        }

        public static void AddTagsToActivity(this Activity activity, string provider, MessageBusMessage message, string entityPath, object messageId, string operationType = "receive", IDictionary<string, object> additionalTags = null)
        {
            if (activity == null) return;

            var correlationid = message.GetCorrelationId();

            activity.SetTag("body", Encoding.UTF8.GetString(message.Body));
            activity.SetTag("messaging.system", provider);
            activity.SetTag("messaging.destination.name", entityPath);
            activity.SetTag("messaging.operation.type", operationType);
            activity.SetTag("propagation_id", activity.Context.TraceId.ToString());
            activity.SetTag("messaging.message.id", messageId);

            if (correlationid.HasValue)
                activity.SetTag("messaging.message.conversation_id", correlationid.Value);

            if (additionalTags is not null)
                foreach (var tag in additionalTags)
                    activity.SetTag(tag.Key, tag.Value);
        }
    }
}
