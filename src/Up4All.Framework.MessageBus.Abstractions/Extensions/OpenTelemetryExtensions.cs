using System.Diagnostics;

namespace Up4All.Framework.MessageBus.Abstractions.Extensions
{
    public static class OpenTelemetryExtensions<TSource> where TSource : class
    {
        public static string ServiceName { get; }
        public static string ServiceVersion { get; }

        static OpenTelemetryExtensions()
        {
            ServiceName = typeof(TSource).Assembly.GetName().Name;
            ServiceVersion = typeof(TSource).Assembly.GetName().Version.ToString();
        }

        public static ActivitySource CreateActivitySource() =>
            new ActivitySource(ServiceName, ServiceVersion);
    }
}
