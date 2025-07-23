namespace Core.Domain.HealthCheckDTOs;

public class HealthCheckResponse
{
    public bool IsEnabled { get; set; } = true;
    public int NumberOfActiveClients { get; set; }
    public DateTime ExpirationTime { get; set; } 
}
