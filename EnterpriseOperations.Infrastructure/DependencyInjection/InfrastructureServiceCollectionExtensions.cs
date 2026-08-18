using EnterpriseOperations.Application.Interfaces;
using EnterpriseOperations.Infrastructure.BackgroundJobs;
using EnterpriseOperations.Infrastructure.Data;
using EnterpriseOperations.Infrastructure.ExternalServices;
using EnterpriseOperations.Infrastructure.Identity;
using EnterpriseOperations.Infrastructure.Repositories;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseOperations.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddDatabase(configuration);
            services.AddRepositories();
            services.AddCacheProvider(configuration);
            services.AddExternalServices(configuration);
            services.AddIdentityServices();

            if (configuration.GetValue<bool>("BackgroundJobs:Enabled")) 
            {
                services.AddBackgroundJobs(configuration);
            }

            return services;
        }

        private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration) 
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("The DefaultConnection connection string is missing.");

            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services) 
        {
            services.AddScoped<IOperationTaskRepository, OperationTaskRepository>();

            return services;
        }

        private static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration) 
        {
            services
                .AddHttpClient<IExternalSystemService, ExternalSystemService>(client =>
                {
                    var baseUrl = configuration["ExternalSystems:OperationsApiBaseUrl"] ?? throw new InvalidOperationException("The external system base URL is missing.");

                    client.BaseAddress = new Uri(baseUrl);
                })
                .AddStandardResilienceHandler(options =>
                {
                    var totalRequestTimeoutSeconds = configuration.GetValue<int>("ExternalSystems:TotalRequestTimeoutSeconds");

                    var attemptTimeoutSeconds = configuration.GetValue<int>("ExternalSystems:AttemptTimeoutSeconds");

                    var retryDelayMilliseconds = configuration.GetValue<int>("ExternalSystems:RetryDelayMilliseconds");

                    var maxRetryAttempts = configuration.GetValue<int>("ExternalSystems:MaxRetryAttempts");

                    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(totalRequestTimeoutSeconds);

                    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(attemptTimeoutSeconds);

                    options.Retry.MaxRetryAttempts = maxRetryAttempts;

                    options.Retry.Delay = TimeSpan.FromMilliseconds(retryDelayMilliseconds);
                });

            return services;
        }

        private static IServiceCollection AddIdentityServices(this IServiceCollection services) 
        {
            services
                .AddIdentityApiEndpoints<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            return services;
        }

        private static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration) 
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("The DefaultConnection connection string is missing.");

            services.AddHangfire(options => options.UseSqlServerStorage(connectionString));

            services.AddHangfireServer();

            services.AddScoped<ExternalSystemStatusJob>();

            return services;
        }
    }
}
