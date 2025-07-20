using Domain;

namespace Application;

public interface IHealthCheckService
{
    HealthCheckResponse Handle(HealthCheckRequest request);
}