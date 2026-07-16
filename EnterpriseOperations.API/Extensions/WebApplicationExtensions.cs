using EnterpriseOperations.Infrastructure.BackgroundJobs;
using EnterpriseOperations.Infrastructure.Data;
using EnterpriseOperations.Infrastructure.Identity;
using Hangfire;
using Microsoft.AspNetCore.Identity;

namespace EnterpriseOperations.API.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task InitializeDatabaseAsync(this WebApplication app) 
        {
            using var scope = app.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await DbInitializer.SeedAsync(dbContext, roleManager, userManager);
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

            app.UseHangfireDashboard("/hangfire");

            RecurringJob.AddOrUpdate<ExternalSystemStatusJob>(
                "external-system-status-check",
                job => job.CheckStatusAsync(),
                Cron.Hourly);

            app.MapIdentityApi<ApplicationUser>();
            app.MapControllers();

            return app;
        }
    }
}
