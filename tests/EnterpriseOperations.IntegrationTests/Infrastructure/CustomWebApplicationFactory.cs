using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace EnterpriseOperations.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    public CustomWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var testConfiguration = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
                ["Cache:Provider"] = "Memory",
                ["BackgroundJobs:Enabled"] = "false",
                ["Database:ApplyMigrationsOnStartup"] = "true",
                ["Database:SeedDevelopmentData"] = "false"
            };

            configurationBuilder.AddInMemoryCollection(testConfiguration);
        });
    }
}
