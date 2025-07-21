using System.Security.AccessControl;

namespace Messaging.gRPC;

public class HealthState
{
    public bool IsEnabled { get; set; } = true;
    public int MaxClients { get; set; } = 5;
    public DateTime ExpirationTime { get; set; } = DateTime.UtcNow.AddMinutes(10);
}