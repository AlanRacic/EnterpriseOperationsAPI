using EnterpriseOperations.Domain.Entities;
using EnterpriseOperations.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            await context.Database.MigrateAsync();

            var roles = new[] { "Admin", "Operator" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            await SeedUserAsync(
                userManager,
                email: "admin@example.com",
                password: "Admin123!",
                role: "Admin");

            await SeedUserAsync(
                userManager,
                email: "operator@example.com",
                password: "Operator123!",
                role: "Operator");

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

            await context.OperationTasks.AddRangeAsync(operationTasks);
            await context.SaveChangesAsync();
        }

        private static async Task SeedUserAsync(UserManager<ApplicationUser> userManager, string email, string password, string role)
        {
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser is not null)
            {
                return;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
