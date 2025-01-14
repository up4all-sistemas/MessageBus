using System.Diagnostics;

namespace Up4All.Framework.MessageBus.Abstractions.Extensions
{
    public static class OpenTelemetryExtensions<TSource> where TSource : class
    {
        public static ActivitySource CreateActivitySource()
        {
            var assemblyName = typeof(TSource).Assembly.GetName();
            return new ActivitySource(assemblyName.Name, assemblyName.Version.ToString());
        }
    }
}
