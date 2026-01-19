using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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
            return CreateActivity(activitySource, properties,  activityName, kind);
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
                    return [Encoding.UTF8.GetString((byte[])x.First(k => k.Key == key).Value)];
                return [];
            });
            Baggage.Current = parent.Baggage;

            return parent;
        }

        public static void AddTagsToActivity(this Activity activity, string provider, byte[] body, string entityPath, IDictionary<string, object> additionalTags = null)
        {
            if (activity == null) return;

            activity.SetTag("message", Encoding.UTF8.GetString(body));
            activity.SetTag("messaging.system", provider);
            activity.SetTag("messaging.destination", entityPath);

            if(additionalTags is not null)
                foreach (var tag in additionalTags)
                    activity.SetTag(tag.Key, tag.Value);
        }
    }
}
