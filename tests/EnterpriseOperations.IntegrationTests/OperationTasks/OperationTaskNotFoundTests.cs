using EnterpriseOperations.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.IntegrationTests.OperationTasks;

[Collection(IntegrationTestCollection.Name)]
public class OperationTaskNotFoundTests
{
    private readonly MsSqlContainerFixture _sqlFixture;

    public OperationTaskNotFoundTests(MsSqlContainerFixture sqlFixture)
    {
        _sqlFixture = sqlFixture;
    }

    [Fact]
    public async Task GetOperationTask_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory(_sqlFixture.ConnectionString);

        var client = factory.CreateClient();

        var uniqueId = Guid.NewGuid().ToString("N");

        var email = $"notfound-user-{uniqueId}@example.com";

        const string password = "Integration123!";

        await factory.CreateUserAsync(email, password);

        var accessToken = await factory.LoginAsync(client, email, password, TestContext.Current.CancellationToken);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        const int nonExistingId = int.MaxValue;

        // Act
        var response = await client.GetAsync($"/api/OperationTasks/{nonExistingId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
