using CodeChallenge.Models;
using CodeChallenge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace CodeChallenge.Controllers
{
    [ApiController]
    [Route("api/compensation")]
    public class CompensationController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ICompensationService _compensationService;

        public CompensationController(ILogger<CompensationController> logger, ICompensationService compensationService)
        {
            _logger = logger;
            _compensationService = compensationService;
        }

        [HttpGet("{id}", Name = "getCompensationByEmployeeId")]
        public IActionResult GetCompensationByEmployeeId(String id)
        {
            _logger.LogDebug($"CompensationController.GetCompensationByEmployeeId: Request received for employee id: {id}");

            var compensations = _compensationService.GetByEmployeeId(id);

            if (compensations == null || compensations.Count() == 0)
                return NotFound();

            return Ok(compensations);
        }

        [HttpPost]
        public IActionResult CreateCompensation([FromBody] Compensation compensation)
        {
            if (compensation == null || compensation.EmployeeId == null)
            {
                return BadRequest("Missing employee id");
            }

            _logger.LogDebug($"CompensationController.CreateCompensation: Request received for employee id: {compensation.EmployeeId}");

            if (_compensationService.Create(compensation) == null)
            {
                return BadRequest($"Employee not found for the specified id {compensation.EmployeeId}");
            }

            // not sure if I would want to return it this way, but I will follow the convention in EmployeeController for this
            return CreatedAtRoute("getCompensationByEmployeeId", new { id = compensation.CompensationId }, compensation);
        }
    }
}
