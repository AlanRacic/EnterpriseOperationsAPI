using EnterpriseOperations.API.Extensions;
using EnterpriseOperations.Application.DependencyInjection;
using EnterpriseOperations.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApiServices();

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.ConfigureApiPipeline();

app.Run();
public partial class Program;
