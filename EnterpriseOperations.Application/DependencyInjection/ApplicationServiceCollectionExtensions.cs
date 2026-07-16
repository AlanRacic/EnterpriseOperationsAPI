using EnterpriseOperations.Application.Interfaces;
using EnterpriseOperations.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseOperations.Application.DependencyInjection
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services) 
        {
            services.AddScoped<IOperationTaskService, OperationTaskService>();

            return services;
        }
    }
}
