using EnterpriseOperations.Application.Interfaces;
using EnterpriseOperations.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Application.Services
{
    public class OperationTaskService : IOperationTaskService
    {
        private readonly IOperationTaskRepository _operationTaskRepository;

        public OperationTaskService(IOperationTaskRepository operationTaskRepository) 
        {
            _operationTaskRepository = operationTaskRepository;
        }

        public async Task<IEnumerable<OperationTaskDto>> GetAllAsync()
        {
            var tasks = await _operationTaskRepository.GetAllAsync();

            return tasks.Select(task => new OperationTaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                CompletedAt = task.CompletedAt
            });
        }
    }
}
