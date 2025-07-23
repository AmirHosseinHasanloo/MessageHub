namespace Core.Domain.HealthCheckDTOs;

public class HealthCheckRequest
{
    public string Id { get; set; }
    public DateTime SystemTime { get; set; }
    public int NumberOfConnectedClients { get; set; }
}