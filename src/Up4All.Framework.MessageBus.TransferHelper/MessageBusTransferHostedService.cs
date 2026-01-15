using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

using Up4All.Framework.MessageBus.Abstractions.Interfaces;

namespace Up4All.Framework.MessageBus.TransferHelper
{
    public class MessageBusTransferHostedService(IHostApplicationLifetime applicationLifetime, IMessageBusTransferClient transferClient, ILogger<MessageBusTransferHostedService> logger) : IHostedService
    {
        private readonly IHostApplicationLifetime _applicationLifetime = applicationLifetime;
        private readonly IMessageBusTransferClient _transferClient = transferClient;
        private readonly ILogger<MessageBusTransferHostedService> _logger = logger;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Transfer client");
            await _transferClient.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Transfer client");
            await _transferClient.StopAsync(cancellationToken);
            _applicationLifetime.StopApplication();
        }
    }
}
