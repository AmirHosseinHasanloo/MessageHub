using Application.Contracts;
using Core.Domain.HealthCheckDTOs;
using Messaging.EventHandler;
using Messaging.Services;
using Microsoft.AspNetCore.Mvc;
using SharedLayer.Common;

namespace MessageHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        private readonly IHealthCheckService _healthCheckService;
        private readonly HealthChecker _healthChecker;
        public HealthCheckController(IHealthCheckService healthCheckService, HealthChecker healthChecker)
        {
            _healthCheckService = healthCheckService;
            _healthChecker = healthChecker;
        }

        [HttpPost("health")]
        public IActionResult CheckHealth([FromBody] HealthCheckRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = _healthChecker.GetCurrentState();
            return Ok(response);
        }
    }
}
