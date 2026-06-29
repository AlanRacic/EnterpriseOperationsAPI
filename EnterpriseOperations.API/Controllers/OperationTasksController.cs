using EnterpriseOperations.Application.DTOs;
using EnterpriseOperations.Application.Interfaces;
using EnterpriseOperations.Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EnterpriseOperations.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OperationTasksController : ControllerBase
    {
        private readonly IOperationTaskService _operationTaskService;

        private readonly ILogger<OperationTasksController> _logger;

        public OperationTasksController(IOperationTaskService operationTaskService, ILogger<OperationTasksController> logger) 
        {
            _operationTaskService = operationTaskService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() 
        {
            var tasks = await _operationTaskService.GetAllAsync();

            return Ok(tasks);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] OperationTaskQueryParameters queryParameters)
        {
            var result = await _operationTaskService.GetPagedAsync(queryParameters);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id) 
        {
            _logger.LogInformation("Retrieving operation task with ID {TaskId}.", id);

            var task = await _operationTaskService.GetByIdAsync(id);

            if (task is null) 
            {
                _logger.LogWarning("Operation task with ID {TaskId} was not found.", id);

                return NotFound();
            }

            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOperationTaskDto dto)
        {
            _logger.LogInformation("Creating a new operation task with title {Title}.", dto.Title);

            var createdTask = await _operationTaskService.CreateAsync(dto);

            _logger.LogInformation("Operation task with ID {TaskId} was created.", createdTask.Id);

            return CreatedAtAction(
                nameof(GetById),
                new {id=createdTask.Id},
                createdTask);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateOperationTaskDto dto) 
        {
            _logger.LogInformation("Updating operation task with ID {TaskId}. IsComplete: {IsCompleted}", id, dto.IsCompleted);

            var updated = await _operationTaskService.UpdateAsync(id, dto);

            if (!updated) 
            {
                _logger.LogWarning("Update failed because operation task with ID {TaskId} was not found.", id);

                return NotFound();
            }

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id) 
        {
            _logger.LogInformation("Deleting operation task with ID {TaskId}.", id);

            var deleted = await _operationTaskService.DeleteAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Delete failed because operation task with ID {TaskId} was not found.", id);

                return NotFound();
            }

            return NoContent();
        }
    }
}
