using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace EnterpriseOperations.IntegrationTests.Health
{
    public class HealthEndpointTests
    {
        [Fact]
        public async Task GetLiveHealthEndpoint_ReturnsOk()
        {
            // Arrange
            await using var factory = new WebApplicationFactory<Program>();

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health/live");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
