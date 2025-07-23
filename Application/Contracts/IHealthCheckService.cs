using Core.Domain.HealthCheckDTOs;

namespace Application.Contracts;

public interface IHealthCheckService
{
    HealthCheckResponse Handle(HealthCheckRequest request);
}