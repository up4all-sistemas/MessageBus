using Microsoft.Extensions.DependencyInjection;

using Up4All.Framework.MessageBus.Abstractions.Options;
using Up4All.Framework.MessageBus.TransferHelper.Pipelines;

namespace Up4All.Framework.MessageBus.TransferHelper.Extensions
{
    public static class IoCExtensions
    {
        public static TransferPipeline<TSourceOptions, TDestinationOptions> AddTransfer<TSourceOptions, TDestinationOptions>(this IServiceCollection services, string configurationKey = "MessageBusTransferOptions")
            where TSourceOptions : MessageBusOptions, new()
            where TDestinationOptions : MessageBusOptions, new()
        {
            return new TransferPipeline<TSourceOptions, TDestinationOptions>(services, configurationKey);
        }
    }

    
}
