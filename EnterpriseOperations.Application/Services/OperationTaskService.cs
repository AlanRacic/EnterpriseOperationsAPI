using EnterpriseOperations.Application.Interfaces;
using EnterpriseOperations.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Application.Services
{
    public class OperationTaskService : IOperationTaskService
    {
        public Task<IEnumerable<OperationTaskDto>> GetAllAsync() 
        {
            var tasks = new List<OperationTaskDto>
            {
                new()
                {
                    Id = 1,
                    Title = "Review external system integration",
                    Description = "Check whether the external API integration is responding correctly.",
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = 2,
                    Title = "Prepare monthly operations report",
                    Description = "Generate a report for operational performance review.",
                    IsCompleted = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    CompletedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            return Task.FromResult<IEnumerable<OperationTaskDto>>(tasks);
        }
    }
}
