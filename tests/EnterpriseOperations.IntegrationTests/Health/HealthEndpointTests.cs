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
        var response = await client.GetAsync("/health/live", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetReadyHealthEndpoint_ReturnsOk_WhenDatabaseIsAvailable() 
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory(_sqlFixture.ConnectionString);

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
