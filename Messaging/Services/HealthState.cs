using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Services
{
    public class HealthState
    {
        public bool IsEnabled { get; set; }
        public int MaxClients { get; set; }
        public DateTime ExpirationTime { get; set; } = DateTime.UtcNow.AddMinutes(10);
    }
}
