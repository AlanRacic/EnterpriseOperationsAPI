using EnterpriseOperations.Application.Interfaces;
using EnterpriseOperations.Application.Services;
using EnterpriseOperations.Infrastructure.Repositories;
using EnterpriseOperations.Infrastructure.Data;
using EnterpriseOperations.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<IOperationTaskService, OperationTaskService>();

builder.Services.AddScoped<IOperationTaskRepository, OperationTaskRepository>();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddStackExchangeRedisCache(options => options.Configuration = builder.Configuration["Redis:ConnectionString"]);

builder.Services.AddScoped<ICacheService, RedisCacheService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope()) 
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.SeedAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
