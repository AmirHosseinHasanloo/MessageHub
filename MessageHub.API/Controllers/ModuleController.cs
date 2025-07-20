using Application;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MessageHub.API.Controllers;

[ApiController]
[Route("api/module")]
public class ModuleController : ControllerBase
{
    private IHealthCheckService _healthCheckService;

    public ModuleController(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpPost("health")]
    public ActionResult CheckHealth([FromBody] HealthCheckRequest request)
    {
        var response = _healthCheckService.Handle(request);

        //TODO : Add fluent Validation.
        return Ok();
    }
}