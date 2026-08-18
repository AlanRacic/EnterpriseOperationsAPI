using EnterpriseOperations.IntegrationTests.Infrastructure;
using System.Net;

namespace EnterpriseOperations.IntegrationTests.Health;

[Collection(IntegrationTestCollection.Name)]
public class HealthEndpointTests
{
    private readonly MsSqlContainerFixture _sqlFixture;

    public HealthEndpointTests(MsSqlContainerFixture sqlFixture)
    {
        _sqlFixture = sqlFixture;
    }

    [Fact]
    public async Task GetLiveHealthEndpoint_ReturnsOk()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory(_sqlFixture.ConnectionString);

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
