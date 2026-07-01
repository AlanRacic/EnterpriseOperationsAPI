using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Hangfire;

namespace EnterpriseOperations.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() 
        {
            return Ok(new 
            {
                Status = "Healthy",
                Application = "Enterprise Operations API",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("error")]
        public IActionResult ThrowError() 
        {
            throw new InvalidOperationException("Test exception for global error handling.");
        }

        [HttpPost("background-test")]
        public IActionResult EnqueueBackgroundTest() 
        {
            BackgroundJob.Enqueue(() => Console.WriteLine("Background job executed from Enterprise Operations API."));

            return Accepted(new { Message = "Background job has been enqueued." });
        }
    }
}
