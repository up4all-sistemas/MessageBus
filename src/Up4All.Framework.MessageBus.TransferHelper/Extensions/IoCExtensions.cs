using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Trace;

using System.Diagnostics;

using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.TransferHelper.Handlers;
using Up4All.Framework.MessageBus.TransferHelper.Pipelines;

namespace Up4All.Framework.MessageBus.TransferHelper.Extensions
{
    public static class IoCExtensions
    {
        public static readonly ActivitySource ActivitySource = OpenTelemetryExtensions.CreateActivitySource<DefaultBeforeTransferHandler>();

        public static TransferPipeline<TSourceOptions, TDestinationOptions> AddTransfer<TSourceOptions, TDestinationOptions>(this IServiceCollection services, string configurationKey = "MessageBusTransferOptions")
            where TSourceOptions : MessageBusOptions, new()
            where TDestinationOptions : MessageBusOptions, new()
        {
            return new TransferPipeline<TSourceOptions, TDestinationOptions>(services, configurationKey);
        }

        public static TracerProviderBuilder AddOpenTelemetryForTransferBusMessageBus(this TracerProviderBuilder builder)
        {
            builder.AddSource(ActivitySource.Name);
            return builder;
        }

        private static string CreateActivityName(this ReceivedMessage arg, string activityName, string entitypath)
        {
            return $"{activityName} {entitypath}";
        }

        private static string CreateMessageReceivedActivityName(this ReceivedMessage arg, string entitypath)
        {
            return arg.CreateActivityName("message-transfer", entitypath);
        }

        public static Activity CreateMessageReceivedActivity(this ReceivedMessage arg, string entitypath)
        {
            var activityName = arg.CreateMessageReceivedActivityName(entitypath);
            return ActivitySource.CreateActivity(arg.UserProperties, activityName, ActivityKind.Consumer);
        }
    }
}
