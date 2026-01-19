using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Up4All.Framework.MessageBus.Abstractions.Options;

namespace Up4All.Framework.MessageBus.TransferHelper.Options
{
    public class TransferOptions<TSourceOptions, TDestinationOptions>
        where TSourceOptions : MessageBusOptions
        where TDestinationOptions : MessageBusOptions
    {
        public TSourceOptions Source { get; set; } = null!;

        public TDestinationOptions Destination { get; set; } = null!;

        public TransferTransformations? Transformations { get; set; }
    }

    public class TransferTransformations
    {
        public IEnumerable<TransferTransformation>? InHeader { get; set; } = null;

        public IEnumerable<TransferTransformation>? InBody { get; set; } = null;
    }

    public class TransferTransformation
    {
        [Required]
        public string Key { get; set; } = null!;

        [Required]
        public string Operation { get; set; } = null!;

        public object? Value { get; set; }
    }
}
