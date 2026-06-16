using EnterpriseOperations.Application.DTOs;
using EnterpriseOperations.Application.Interfaces;
using EnterpriseOperations.Application.Models;
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

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] OperationTaskQueryParameters queryParameters)
        {
            var result = await _operationTaskService.GetPagedAsync(queryParameters);

            return Ok(result);
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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateOperationTaskDto dto) 
        {
            var updated = await _operationTaskService.UpdateAsync(id, dto);

            if (!updated) 
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id) 
        {
            var deleted = await _operationTaskService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
