using EnterpriseOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await context.Database.MigrateAsync();

            if (await context.OperationTasks.AnyAsync()) 
            {
                return;
            }

            var operationTasks = new List<OperationTask>
            {
                new() 
                {
                    Title = "Review external system integration",
                    Description = "Check whether the external API integration is repsonding correctly.",
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Title = "Prepare monthly operations report",
                    Description = "Generate a report for operational performance review.",
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Title = "Verify data syncronization",
                    Description = "Validate that data from the external system is syncronized correctly.",
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Title = "Archive completed operation logs",
                    Description = "Move old completed operation logs to archive storage.",
                    IsCompleted = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    CompletedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            await context.OperationTasks.AddRangeAsync(operationTasks);
            await context.SaveChangesAsync();
        }
    }
}
