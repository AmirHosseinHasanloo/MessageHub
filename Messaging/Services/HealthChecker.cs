using Core.Domain.HealthCheckDTOs;
using Messaging.EventHandler;
using Messaging.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Messaging.Services;
public class HealthChecker : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _healthUrl;
    private readonly string _id;
    private readonly ILogger<HealthChecker> _logger;
    private readonly ClientManager _clientManager;
    private Timer _timer;
    private HealthCheckResponse _currentState = new();

    public HealthChecker(HttpClient httpClient, string healthUrl, string id, ILogger<HealthChecker> logger, ClientManager clientManager)
    {
        _httpClient = httpClient;
        _healthUrl = healthUrl;
        _id = id;
        _logger = logger;
        _clientManager = clientManager;

        _timer = new Timer(async _ => await CheckHealthAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    public HealthCheckResponse GetCurrentState() => _currentState;

    private async Task CheckHealthAsync()
    {
        var request = new HealthCheckRequest
        {
            Id = _id,
            SystemTime = DateTime.UtcNow,
            NumberOfConnectedClients = _clientManager.GetActiveCount()
        };

        for (int attempt = 1; attempt <= 5; attempt++)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_healthUrl, request);
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();
                    if (body != null)
                    {
                        _currentState = body;
                        _logger.LogInformation($"Health check success: IsEnabled={body.IsEnabled}, ActiveClients={body.NumberOfActiveClients}");
                        return;
                    }
                }
                else
                {
                    _logger.LogWarning($"Health check failed with status code {response.StatusCode}. Attempt {attempt}/5");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Health check exception: {ex.Message}. Attempt {attempt}/5");
            }

            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        _currentState.IsEnabled = false;
        _logger.LogError("Health check failed 5 times. Service will be disabled.");
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}