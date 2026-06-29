using EnterpriseOperations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context, RoleManager<IdentityRole> roleManager)
        {
            await context.Database.MigrateAsync();

            if (await context.OperationTasks.AnyAsync()) 
            {
                return;
            }

            var operationTasks = new List<OperationTask>();

            var titles = new[]
            {
                "Review external system integration",
                "Prepare monthly operations report",
                "Verify data synchronization",
                "Archive completed operation logs",
                "Investigate delayed API response",
                "Validate customer import process",
                "Check background job status",
                "Review failed payment synchronization",
                "Analyze performance report",
                "Prepare data quality summary"
            };

            var descriptions = new[]
            {
                "Check whether the external API integration is responding correctly.",
                "Generate a report for operational performance review.",
                "Validate that data from the external system is synchronized correctly.",
                "Move old completed operation logs to archive storage.",
                "Analyze why the external integration sometimes responds slowly.",
                "Verify that imported customer records match the expected format.",
                "Review whether scheduled background jobs completed successfully.",
                "Investigate why payment data was not synchronized.",
                "Analyze response times and identify potential bottlenecks.",
                "Prepare a summary of data quality issues for review."
            };

            for (int i = 1; i < 150; i++)
            {
                var isCompleted = i % 3 == 0;

                operationTasks.Add(new OperationTask
                {
                    Title = $"{titles[i % titles.Length]} #{i}",
                    Description = descriptions[i % descriptions.Length],
                    IsCompleted = isCompleted,
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    CompletedAt = isCompleted ? DateTime.UtcNow.AddDays(-i + 1) : null
                });
            }

            var roles = new[] { "Admin", "Operator" };

            foreach (var role in roles) 
            {
                if (!await roleManager.RoleExistsAsync(role)) 
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            await context.OperationTasks.AddRangeAsync(operationTasks);
            await context.SaveChangesAsync();
        }
    }
}
