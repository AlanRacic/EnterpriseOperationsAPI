using EnterpriseOperations.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseOperations.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalSystemController : ControllerBase
    {
        private readonly IExternalSystemService _externalSystemService;

        public ExternalSystemController(IExternalSystemService externalSystemService) 
        {
            _externalSystemService = externalSystemService;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus() 
        {
            var status = await _externalSystemService.GetStatusAsync();

            return Ok(status);
        }
    }
}
