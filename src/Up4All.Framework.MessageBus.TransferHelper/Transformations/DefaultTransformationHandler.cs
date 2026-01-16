using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Extensions;
using Up4All.Framework.MessageBus.Abstractions.Messages;
using Up4All.Framework.MessageBus.TransferHelper.Consts;
using Up4All.Framework.MessageBus.TransferHelper.Options;

namespace Up4All.Framework.MessageBus.TransferHelper.Transformations
{
    public class DefaultTransformationHandler(ILogger<DefaultTransformationHandler> logger) : ITransformationHandler
    {
        protected readonly ILogger<DefaultTransformationHandler> _logger = logger;

        public virtual Task<MessageBusMessage> TransformAsync(ReceivedMessage receivedMessage, TransferTransformations? transformationOptions, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Running default Transformation");

            MessageBusMessage mesage = receivedMessage;
            if (transformationOptions is null) return Task.FromResult(mesage);

            if (transformationOptions.InHeader is not null)            
                ProcessHeader(mesage, transformationOptions.InHeader!);                

            if (transformationOptions.InBody is null) return Task.FromResult(mesage);

            return Task.FromResult(mesage);
        }

        private static void ProcessHeader(MessageBusMessage message, IEnumerable<TransferTransformation> transformations)
        {   
            foreach(var transformation in transformations)
            {
                if (!message.UserProperties.ContainsKey(transformation.Key)) continue;

                if (transformation?.Operation == Operation.Remove)
                    message.RemoveUserProperty(transformation.Key);

                if(transformation?.Operation == Operation.Add)
                    message.AddUserProperty(transformation!.Key, transformation.Value!);

                if (transformation?.Operation == Operation.Update)
                    message.UserProperties[transformation.Key] = transformation.Value!;

                if (transformation?.Operation == Operation.ChangeKey
                    && message.TryGetUserPropertyValue(transformation.Key, out var value))
                {
                    message.RemoveUserProperty(transformation.Key);
                    message.AddUserProperty(transformation!.Value!.ToString(), value);
                }
            }            
        }
    }
}
