using Application.Contracts;
using Core.Domain.HealthCheckDTOs;
using Messaging.EventHandler;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class GrpcHealthChecker : IGrpcHealthChecker, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _healthUrl;
        private readonly string _id;
        private readonly ILogger<GrpcHealthChecker> _logger;
        private readonly GrpcClientManager _clientManager;
        private Timer _timer;
        private HealthCheckResponse _CurrentState = new();

        public GrpcHealthChecker(HttpClient httpClient,
       string healthUrl,    
       string id,
       ILogger<GrpcHealthChecker> logger,
       GrpcClientManager clientManager)
        {
            _httpClient = httpClient;
            _healthUrl = healthUrl;   
            _id = id;
            _logger = logger;
            _clientManager = clientManager;

            _timer = new Timer(async _ => await CheckHealthAsync(), null,
                TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        private async Task CheckHealthAsync()
        {
            var request = new HealthCheckRequest
            {
                Id = _id,
                NumberOfConnectedClients = _clientManager.GetActiveCount(),
                SystemTime = DateTime.UtcNow,
            };

            for (int attempt = 1; attempt < 5; attempt++)
            {
                try
                {
                    var response = await _httpClient
                        .PostAsJsonAsync(_healthUrl, request);

                    if (response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();
                        if (body != null)
                        {
                            _CurrentState = body;
                            _logger.LogInformation($"Health check success : " +
                                $"IsEnabled={body.IsEnabled}, ActiveClients={body.NumberOfActiveClients}");
                            return;
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Health check failed with status code " +
                            $": {response.StatusCode}. Attempt {attempt}/5");
                    }
                }
                catch (Exception ex)
                {

                    _logger.LogError($"Health check exception: {ex.Message}. Attempt {attempt}/5");
                }

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
            _CurrentState.IsEnabled = false;
            _logger.LogError($"Health check failed 5 times. Service will be disabled.");
        }

        public HealthCheckResponse GetCurrentState() => _CurrentState;

        public void RegisterClient(string clientId) => _clientManager.RegisterClient(clientId);

        public void CheckInactiveClients() => _clientManager.CheckInactiveClients();

        public void MarkClientActive(string clientId) => _clientManager.MarkClientActive(clientId);


        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
