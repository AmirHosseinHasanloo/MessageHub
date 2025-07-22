using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Messaging.EventHandler
{
    public class ClientCleanupService : BackgroundService
    {
        private readonly ClientManager _clientManager;
        private readonly ILogger<ClientCleanupService> _logger;

        public ClientCleanupService(ClientManager clientManager, ILogger<ClientCleanupService> logger)
        {
            _clientManager = clientManager;
            _logger = logger;
        }




        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Client CleanUp Service Started ...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _clientManager.CheckInActiveClients();
                    _logger.LogInformation
                        ($"Inactive clients cleaned up. Active Clients : " +
                        $"{_clientManager.GetActiveCount()}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"error during cleaning inactive clients {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

            _logger.LogInformation("Client CleanUp Service stopped ! ...");
        }
    }
}
