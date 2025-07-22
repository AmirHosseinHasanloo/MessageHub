using Application;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MessageHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private IHealthCheckService _healthCheckService;

        public HealthCheckController(IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        [HttpPost("health")]
        public ActionResult CheckHealth([FromBody] HealthCheckRequest request)
        {
            var response = _healthCheckService.Handle(request);

            //TODO : Add fluent Validation.
            return Ok(response);
        }
    }
}
