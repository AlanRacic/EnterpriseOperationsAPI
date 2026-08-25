using EnterpriseOperations.Application.DTOs;
using EnterpriseOperations.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EnterpriseOperations.IntegrationTests.OperationTasks;

[Collection(IntegrationTestCollection.Name)]
public class OperationTaskConcurrencyTests
{
    private readonly MsSqlContainerFixture _sqlFixture;

    public OperationTaskConcurrencyTests(MsSqlContainerFixture sqlFixture)
    {
        _sqlFixture = sqlFixture;
    }

    [Fact]
    public async Task UpdateOperationTask_WithStaleRowVersion_ReturnsConflict()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory(_sqlFixture.ConnectionString);

        var client = factory.CreateClient();

        var uniqueId = Guid.NewGuid().ToString("N");

        var email = $"concurrency-admin-{uniqueId}@example.com";

        const string password = "Admin123!";

        await factory.CreateUserWithRoleAsync(email, password, "Admin");

        var accessToken = await factory.LoginAsync(client, email, password, TestContext.Current.CancellationToken);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createDto = new CreateOperationTaskDto
        {
            Title = $"Concurrency task {uniqueId}",
            Description = "Task used for concurrency integration testing."
        };

        var createResponse = await client.PostAsJsonAsync("/api/OperationTasks", createDto, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdTask = await createResponse.Content.ReadFromJsonAsync<OperationTaskDto>(TestContext.Current.CancellationToken);

        Assert.NotNull(createdTask);

        var originalRowVersion = createdTask.RowVersion;

        Assert.False(string.IsNullOrWhiteSpace(originalRowVersion));

        // First update using the current RowVersion
        var firstUpdateDto = new UpdateOperationTaskDto
        {
            Title = $"First update {uniqueId}",
            Description = "First successful update.",
            IsCompleted = false,
            RowVersion = originalRowVersion
        };

        var firstUpdateResponse = await client.PutAsJsonAsync($"/api/OperationTasks/{createdTask.Id}", firstUpdateDto, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, firstUpdateResponse.StatusCode);

        // Second update intentionally uses the stale RowVersion
        var staleUpdateDto = new UpdateOperationTaskDto
        {
            Title = $"Stale update {uniqueId}",
            Description = "This update should cause a concurrency conflict.",
            IsCompleted = true,
            RowVersion = originalRowVersion
        };

        // Act
        var staleUpdateResponse = await client.PutAsJsonAsync($"/api/OperationTasks/{createdTask.Id}", staleUpdateDto, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, staleUpdateResponse.StatusCode);
    }
}
