using System.Net.Http.Json;
using Domain;
using Microsoft.Extensions.Logging;

namespace Messaging.gRPC;

public class HealthChecker
{
    private readonly HttpClient _httpClient;
    private readonly string _healthUrl;
    private readonly string _Id;
    private readonly Timer _timer;
    private readonly HealthState _state = new();
    private readonly ILogger<HealthChecker> _logger;

    public HealthChecker(HttpClient httpClient, string healthUrl, string id)
    {
        _httpClient = httpClient;
        _healthUrl = healthUrl;
        _Id = id;
        // Schedule periodic check +=>
        _timer = new Timer(async _ => await CheckHealthAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    public HealthChecker(Logger<HealthChecker> logger)
    {
        _logger = logger;
    }

    public HealthState GetCurrentState() => _state;

    private async Task CheckHealthAsync()
    {
        var request = new HealthCheckRequest
        {
            Id = _Id,
            SystemTime = DateTime.UtcNow,
            NumberOfConnectedClients = 5
        };

        for (int attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_healthUrl, request);
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

                    if (body is not null)
                    {
                        _state.IsEnabled = body.IsEnabled;
                        _state.MaxClients = body.NumberOfActiveClients;
                        _state.ExpirationTime = body.ExpirationTime;
                    }

                    return;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"there is an Exception, with this detial : {e.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        _state.IsEnabled = false;
    }
}