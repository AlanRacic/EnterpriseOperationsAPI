using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace EnterpriseOperations.IntegrationTests.Infrastructure
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                var testConfiguration = new Dictionary<string, string?>
                {
                    ["Cache:Provider"] = "Memory",
                    ["BackgroundJobs:Enabled"] = "false",
                    ["Database:ApplyMigrationsOnStartup"] = "false",
                    ["Database:SeedDevelopmentData"] = "false"
                };

                configurationBuilder.AddInMemoryCollection(testConfiguration);
            });
        }
    }
}
