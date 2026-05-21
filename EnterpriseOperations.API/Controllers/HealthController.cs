using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    }
}
