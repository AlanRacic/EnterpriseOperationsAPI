using EnterpriseOperations.API.Middleware;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace EnterpriseOperations.API.Extensions
{
    public static class ApiServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services) 
        {
            services.AddControllers();
            services.AddOpenApi();

            services.AddProblemDetails();
            services.AddExceptionHandler<GlobalExceptionHandler>();

            services.AddOpenTelemetry()
                .ConfigureResource(resource =>
            {
                resource.AddService(
                    serviceName: "EnterpriseOperations.API",
                    serviceVersion: "1.0.0");
            })
                .WithTracing(tracing =>
                {
                    tracing
                        .AddSource("EnterpriseOperations.Application")
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddConsoleExporter();
                });

            return services;
        }
    }
}
