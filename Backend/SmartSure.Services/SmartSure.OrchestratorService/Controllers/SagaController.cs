using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SmartSure.OrchestratorService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SagaController : ControllerBase
    {
        // POST api/saga/start
        [HttpPost("start")]
        public async Task<IActionResult> StartSaga([FromBody] SagaStartRequest request)
        {
            // TODO: Publish saga start event to RabbitMQ
            // Example: Claim processing or policy purchase
            return Ok(new { message = "Saga started", request });
        }
    }

    public class SagaStartRequest
    {
        public string WorkflowType { get; set; } = string.Empty;
        public string ReferenceId { get; set; } = string.Empty;
        // Add more fields as needed
    }
}
