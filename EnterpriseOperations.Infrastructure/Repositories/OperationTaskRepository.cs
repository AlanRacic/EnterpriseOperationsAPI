using EnterpriseOperations.Application.Interfaces;
using EnterpriseOperations.Domain.Entities;
using EnterpriseOperations.Infrastructure.Data;
using EnterpriseOperations.Application.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Infrastructure.Repositories
{
    public class OperationTaskRepository : IOperationTaskRepository
    {
        private readonly AppDbContext _context;

        public OperationTaskRepository(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<IEnumerable<OperationTask>> GetAllAsync() 
        {
            return await _context.OperationTasks
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<OperationTask?> GetByIdAsync(int id) 
        {
            return await _context.OperationTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(task => task.Id == id);
        }

        public async Task<OperationTask> AddAsync(OperationTask operationTask) 
        {
            await _context.OperationTasks.AddAsync(operationTask);
            await _context.SaveChangesAsync();

            return operationTask;
        }

        public async Task<bool> UpdateAsync(OperationTask operationTask) 
        {
            var existingTask = await _context.OperationTasks
                .FirstOrDefaultAsync(task => task.Id == operationTask.Id);

            if (existingTask is null) 
            {
                return false;
            }

            existingTask.Title = operationTask.Title;
            existingTask.Description = operationTask.Description;
            existingTask.IsCompleted = operationTask.IsCompleted;
            existingTask.CompletedAt = operationTask.CompletedAt;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _context.OperationTasks
                .FirstOrDefaultAsync(task => task.Id == id);

            if (task is null) 
            {
                return false;
            }

            _context.OperationTasks.Remove(task);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResult<OperationTask>> GetPagedAsync(OperationTaskQueryParameters queryParameters)
        {
            var query = _context.OperationTasks
                .AsNoTracking()
                .AsQueryable();

            if (queryParameters.IsCompleted.HasValue)
            {
                query = query.Where(task => task.IsCompleted == queryParameters.IsCompleted.Value);
            }

            if (!string.IsNullOrWhiteSpace(queryParameters.SearchTerm))
            {
                var searchTerm = queryParameters.SearchTerm.ToLower();

                query = query.Where(task =>
                    task.Title.ToLower().Contains(searchTerm) ||
                    (task.Description != null && task.Description.ToLower().Contains(searchTerm)));
            }

            query = queryParameters.SortBy.ToLower() switch
            {
                "title" => queryParameters.SortDirection.ToLower() == "asc"
                    ? query.OrderBy(task => task.Title)
                    : query.OrderByDescending(task => task.Title),

                "iscompleted" => queryParameters.SortDirection.ToLower() == "asc"
                    ? query.OrderBy(task => task.IsCompleted)
                    : query.OrderByDescending(task => task.IsCompleted),

                _ => queryParameters.SortDirection.ToLower() == "asc"
                    ? query.OrderBy(task => task.CreatedAt)
                    : query.OrderByDescending(task => task.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .ToListAsync();

            return new PagedResult<OperationTask>
            {
                Items = items,
                PageNumber = queryParameters.PageNumber,
                PageSize = queryParameters.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
