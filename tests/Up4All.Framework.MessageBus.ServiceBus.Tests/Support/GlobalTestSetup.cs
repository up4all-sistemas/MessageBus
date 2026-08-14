using System.Diagnostics;

using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Up4All.Framework.MessageBus.ServiceBus.Tests
{
    /// <summary>
    /// Runs once for the whole assembly. The library relies on
    /// Propagators.DefaultTextMapPropagator (a no-op unless configured) and on
    /// ActivitySource having a listener to actually produce Activities - both are
    /// normally wired up by the hosting OpenTelemetry SDK setup, so tests configure
    /// them explicitly here.
    /// </summary>
    [SetUpFixture]
    public class GlobalTestSetup
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(
                [new TraceContextPropagator(), new BaggagePropagator()]));

            var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
                SampleUsingParentId = (ref ActivityCreationOptions<string> options) => ActivitySamplingResult.AllData
            };
            ActivitySource.AddActivityListener(listener);
        }
    }
}
