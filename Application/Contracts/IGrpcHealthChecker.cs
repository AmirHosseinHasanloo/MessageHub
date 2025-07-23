using Core.Domain.HealthCheckDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface IGrpcHealthChecker
    {
        HealthCheckResponse GetCurrentState();
        void RegisterClient(string clientId);
        void MarkClientActive(string clientId);
        void CheckInactiveClients();
    }
}
