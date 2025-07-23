using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.EventHandler
{
    public class GrpcClientManager
    {
        private readonly Dictionary<string, DateTime> _clientLastSeen = new();
        private readonly List<string> _inactiveClients = new();

        public void RegisterClient(string clientId)
        {
            _clientLastSeen[clientId] = DateTime.UtcNow;
        }

        public void MarkClientActive(string clientId)
        {
            _clientLastSeen[clientId] = DateTime.UtcNow;
        }

        public void CheckInactiveClients()
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in _clientLastSeen.ToList())
            {
                if ((now - kvp.Value).TotalMinutes > 5)
                {
                    _inactiveClients.Add(kvp.Key);
                    _clientLastSeen.Remove(kvp.Key);
                }
            }
        }
        public int GetActiveCount() => _clientLastSeen.Count;
        public List<string> GetInactiveClients() => _inactiveClients.ToList();
    }
}
