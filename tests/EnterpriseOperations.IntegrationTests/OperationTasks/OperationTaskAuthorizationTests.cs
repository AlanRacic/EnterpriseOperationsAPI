using EnterpriseOperations.IntegrationTests.Infrastructure;
using System.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.IntegrationTests.OperationTasks;

[Collection(IntegrationTestCollection.Name)]
public class OperationTaskAuthorizationTests
{
    private readonly MsSqlContainerFixture _sqlFixture;

    public OperationTaskAuthorizationTests(MsSqlContainerFixture sqlFixture)
    {
        _sqlFixture = sqlFixture;
    }

    [Fact]
    public async Task GetOperationTasks_WithoutAuthentication_ReturnsUnauthorized() 
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory(_sqlFixture.ConnectionString);

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/OperationTasks", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
