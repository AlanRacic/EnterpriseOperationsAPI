using EnterpriseOperations.Application.DTOs;
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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id) 
        {
            var task = await _operationTaskService.GetByIdAsync(id);

            if (task is null) 
            {
                return NotFound();
            }

            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOperationTaskDto dto)
        {
            var createdTask = await _operationTaskService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new {id=createdTask.Id},
                createdTask);
        }
    }
}
