using EnterpriseOperations.IntegrationTests.Infrastructure;
using System.Net;

namespace EnterpriseOperations.IntegrationTests.Health
{
    public class HealthEndpointTests
    {
        [Fact]
        public async Task GetLiveHealthEndpoint_ReturnsOk()
        {
            // Arrange
            await using var factory = new CustomWebApplicationFactory();

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health/live");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
