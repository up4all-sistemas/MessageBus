using System.ComponentModel.DataAnnotations;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.TransferHelper.Options
{
    public class TransferOptions<TSourceOptions, TDestinationOptions>
        where TSourceOptions : MessageBusOptions, new()
        where TDestinationOptions : MessageBusOptions, new()
    {
        [Required]
        public TSourceOptions Source { get; set; } = null!;

        [Required]
        public TDestinationOptions Destination { get; set; } = null!;
    }
}
