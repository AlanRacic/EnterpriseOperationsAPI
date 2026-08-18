using EnterpriseOperations.Infrastructure.BackgroundJobs;
using EnterpriseOperations.Infrastructure.Data;
using EnterpriseOperations.Infrastructure.Identity;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EnterpriseOperations.API.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task InitializeDatabaseAsync(this WebApplication app) 
        {
            await using var scope = app.Services.CreateAsyncScope();

            var applyMigrations = app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");

            var seedDevelopmentData = app.Configuration.GetValue<bool>("Database:SeedDevelopmentData");

            if (!applyMigrations && !seedDevelopmentData)
            {
                return;
            }

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (applyMigrations)
            {
                await dbContext.Database.MigrateAsync();
            }

            if (seedDevelopmentData) 
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                await DbInitializer.SeedDevelopmentDataAsync(dbContext, roleManager, userManager);
            }  
        }

        public static WebApplication ConfigureApiPipeline(this WebApplication app) 
        {
            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            if (app.Configuration.GetValue<bool>("BackgroundJobs:Enabled")) 
            {
                app.UseHangfireDashboard("/hangfire");

                RecurringJob.AddOrUpdate<ExternalSystemStatusJob>(
                    "external-system-status-check",
                    job => job.CheckStatusAsync(),
                    Cron.Hourly);
            }
            
            app.MapIdentityApi<ApplicationUser>();

            app.MapHealthChecks(
                "/health/live",
                new HealthCheckOptions
                {
                    Predicate = _ => false
                })
                .AllowAnonymous();

            app.MapHealthChecks(
                "/health/ready",
                new HealthCheckOptions
                {
                    Predicate = healthCheck =>
                        healthCheck.Tags.Contains("ready")
                })
                .AllowAnonymous();

            app.MapControllers();

            return app;
        }
    }
}
