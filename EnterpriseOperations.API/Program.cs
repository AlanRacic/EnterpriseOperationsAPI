using EnterpriseOperations.Application.Interfaces;
using EnterpriseOperations.Application.Services;
using EnterpriseOperations.Application.Settings;
using EnterpriseOperations.Infrastructure.Repositories;
using EnterpriseOperations.Infrastructure.Data;
using EnterpriseOperations.Infrastructure.Caching;
using EnterpriseOperations.Infrastructure.ExternalServices;
using EnterpriseOperations.Infrastructure.Identity;
using EnterpriseOperations.API.Middleware;
using EnterpriseOperations.Infrastructure.BackgroundJobs;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Http.Resilience;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<IOperationTaskService, OperationTaskService>();

builder.Services.AddScoped<IOperationTaskRepository, OperationTaskRepository>();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var redisConnectionString = builder.Configuration["Redis:ConnectionString"];

builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString!));

builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddHttpClient<IExternalSystemService, ExternalSystemService>(client =>
{
    var baseUrl = builder.Configuration["ExternalSystems:OperationsApiBaseUrl"];

    client.BaseAddress = new Uri(baseUrl!);
})
.AddStandardResilienceHandler(options => 
{
    var totalRequestTimeoutSeconds = builder.Configuration.GetValue<int>("ExternalSystems:TotalRequestTimeoutSeconds");

    var attemptTimeoutSeconds = builder.Configuration.GetValue<int>("ExternalSystems:AttemptTimeoutSeconds");

    var retryDelayMiliseconds = builder.Configuration.GetValue<int>("ExternalSystems:RetryDelayMilliseconds");

    var maxRetryAttempts = builder.Configuration.GetValue<int>("ExternalSystems:MaxRetryAttempts");

    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(totalRequestTimeoutSeconds);

    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(attemptTimeoutSeconds);

    options.Retry.MaxRetryAttempts = maxRetryAttempts;

    options.Retry.Delay = TimeSpan.FromMilliseconds(retryDelayMiliseconds);
});

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddIdentityApiEndpoints<ApplicationUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddOpenTelemetry()
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

builder.Services.AddHangfire(configuration => configuration.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<ExternalSystemStatusJob>();

builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("Cache"));

var app = builder.Build();

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope()) 
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    await DbInitializer.SeedAsync(dbContext, roleManager, userManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<ExternalSystemStatusJob>("external-system-status-check", job => job.CheckStatusAsync(), Cron.Hourly);

app.MapIdentityApi<ApplicationUser>();

app.MapControllers();

app.Run();
