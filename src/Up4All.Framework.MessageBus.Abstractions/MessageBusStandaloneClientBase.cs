using System;
using System.Diagnostics;

using Up4All.Framework.MessageBus.Abstractions.Extensions;

namespace Up4All.Framework.MessageBus.Abstractions
{
    public abstract class MessageBusStandaloneClientBase : IDisposable
    {
        protected MessageBusStandaloneClientBase()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        protected void CreateOpenTelemetryActivitySource<TSource>(string activityName, ActivityKind kind) where TSource : class
        {
            var activity = OpenTelemetryExtensions<TSource>.CreateActivitySource()
                .StartActivity(activityName, kind);
        }
    }
}
