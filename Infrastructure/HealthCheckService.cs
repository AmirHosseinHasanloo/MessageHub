using Application.Contracts;
using Core.Domain.HealthCheckDTOs;

namespace Infrastructure;

public class HealthCheckService : IHealthCheckService
{
    private Random _random = new Random();

    public HealthCheckResponse Handle(HealthCheckRequest request)
    {
        return new HealthCheckResponse
        {
            IsEnabled = true,
            NumberOfActiveClients = _random.Next(0, 6),
            ExpirationTime = DateTime.UtcNow.AddMinutes(10)
        };
    }
}