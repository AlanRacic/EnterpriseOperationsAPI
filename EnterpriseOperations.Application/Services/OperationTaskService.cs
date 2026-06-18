using EnterpriseOperations.Application.Interfaces;
using EnterpriseOperations.Application.DTOs;
using EnterpriseOperations.Domain.Entities;
using EnterpriseOperations.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Application.Services
{
    public class OperationTaskService : IOperationTaskService
    {
        private readonly IOperationTaskRepository _operationTaskRepository;

        private readonly ICacheService _cacheService;

        public OperationTaskService(
            IOperationTaskRepository operationTaskRepository,
            ICacheService cacheService) 
        {
            _operationTaskRepository = operationTaskRepository;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<OperationTaskDto>> GetAllAsync()
        {
            var tasks = await _operationTaskRepository.GetAllAsync();

            return tasks.Select(MapToDto);
        }

        public async Task<OperationTaskDto?> GetByIdAsync(int id)
        {
            var task = await _operationTaskRepository.GetByIdAsync(id);

            if (task is null)
            {
                return null;
            }

            return MapToDto(task);
        }

        public async Task<OperationTaskDto> CreateAsync(CreateOperationTaskDto dto)
        {
            var operationTask = new OperationTask
            {
                Title = dto.Title,
                Description = dto.Description,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            var createdTask = await _operationTaskRepository.AddAsync(operationTask);

            return MapToDto(createdTask);
        }

        public async Task<bool> UpdateAsync(int id, UpdateOperationTaskDto dto) 
        {
            var operationTask = new OperationTask
            {
                Id = id,
                Title = dto.Title,
                Description = dto.Description,
                IsCompleted = dto.IsCompleted,
                CompletedAt = dto.IsCompleted ? DateTime.UtcNow : null
            };

            return await _operationTaskRepository.UpdateAsync(operationTask);
        }

        public async Task<bool> DeleteAsync(int id) 
        {
            return await _operationTaskRepository.DeleteAsync(id);
        }

        private static OperationTaskDto MapToDto(OperationTask task) 
        {
            return new OperationTaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                CompletedAt = task.CompletedAt
            };
        }

        public async Task<PagedResult<OperationTaskDto>> GetPagedAsync(OperationTaskQueryParameters queryParameters)
        {
            var cacheKey =
                $"operation-tasks:paged:" +
                $"pageNumber={queryParameters.PageNumber}:" +
                $"pageSize={queryParameters.PageSize}:" +
                $"isCompleted={queryParameters.IsCompleted}:" +
                $"searchTerm={queryParameters.SearchTerm}:" +
                $"sortBy={queryParameters.SortBy}:" +
                $"sortDirection={queryParameters.SortDirection}";

            var cachedResult = await _cacheService.GetAsync<PagedResult<OperationTaskDto>>(cacheKey);

            if (cachedResult is not null)
            {
                return cachedResult;
            }

            var pagedTasks = await _operationTaskRepository.GetPagedAsync(queryParameters);

            var result = new PagedResult<OperationTaskDto>
            {
                Items = pagedTasks.Items.Select(MapToDto),
                PageNumber = pagedTasks.PageNumber,
                PageSize = pagedTasks.PageSize,
                TotalCount = pagedTasks.TotalCount
            };

            await _cacheService.SetAsync(
                cacheKey,
                result,
                TimeSpan.FromMinutes(1));

            return result;
        }
    }
}
