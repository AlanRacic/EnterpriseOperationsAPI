using EnterpriseOperations.Infrastructure.BackgroundJobs;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseOperations.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BackgroundJobsController : ControllerBase
    {
        [HttpPost("external-system-status")]
        public IActionResult EnqueueExternalSystemStatusJob()
        {
            BackgroundJob.Enqueue<ExternalSystemStatusJob>(job => job.CheckStatusAsync());

            return Accepted(new { Message = "External system status job has been enqueued." });
        }
    }
}
