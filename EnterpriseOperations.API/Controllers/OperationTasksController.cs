using EnterpriseOperations.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseOperations.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperationTasksController : ControllerBase
    {
        private readonly IOperationTaskService _operationTaskService;

        public OperationTasksController(IOperationTaskService operationTaskService) 
        {
            _operationTaskService = operationTaskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() 
        {
            var tasks = await _operationTaskService.GetAllAsync();

            return Ok(tasks);
        }
    }
}
