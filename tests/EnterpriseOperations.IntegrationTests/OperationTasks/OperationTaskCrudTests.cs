using EnterpriseOperations.Application.DTOs;
using EnterpriseOperations.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.IntegrationTests.OperationTasks;

[Collection(IntegrationTestCollection.Name)]
public class OperationTaskCrudTests
{
    private readonly MsSqlContainerFixture _sqlFixture;

    public OperationTaskCrudTests(MsSqlContainerFixture sqlFixture)
    {
        _sqlFixture = sqlFixture;
    }

    [Fact]
    public async Task OperationTask_FullCrudFlow_WorksCorrectly()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory(_sqlFixture.ConnectionString);

        var client = factory.CreateClient();

        var uniqueId = Guid.NewGuid().ToString("N");

        var email = $"crud-admin-{uniqueId}@example.com";

        const string password = "Admin123!";

        await factory.CreateUserWithRoleAsync(email, password, "Admin");

        var accessToken = await factory.LoginAsync(client, email, password, TestContext.Current.CancellationToken);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createDto = new CreateOperationTaskDto
        {
            Title = $"Integration task {uniqueId}",
            Description = "Created by the full CRUD integration test."
        };

        // Act + Assert: CREATE
        var createResponse = await client.PostAsJsonAsync("/api/OperationTasks", createDto, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdTask = await createResponse.Content.ReadFromJsonAsync<OperationTaskDto>(TestContext.Current.CancellationToken);

        Assert.NotNull(createdTask);
        Assert.True(createdTask.Id > 0);
        Assert.Equal(createDto.Title, createdTask.Title);
        Assert.Equal(createDto.Description, createdTask.Description);
        Assert.False(createdTask.IsCompleted);
        Assert.False(string.IsNullOrWhiteSpace(createdTask.RowVersion));

        // Act + Assert: GET
        var getResponse = await client.GetAsync($"/api/OperationTasks/{createdTask.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetchedTask = await getResponse.Content.ReadFromJsonAsync<OperationTaskDto>(TestContext.Current.CancellationToken);

        Assert.NotNull(fetchedTask);
        Assert.Equal(createdTask.Id, fetchedTask.Id);
        Assert.Equal(createDto.Title, fetchedTask.Title);

        // Arrange UPDATE
        var originalRowVersion = fetchedTask.RowVersion;

        var updateDto = new UpdateOperationTaskDto
        {
            Title = $"Updated integration task {uniqueId}",
            Description = "Updated by the full CRUD integration test.",
            IsCompleted = true,
            RowVersion = originalRowVersion
        };

        // Act + Assert: UPDATE
        var updateResponse = await client.PutAsJsonAsync($"/api/OperationTasks/{createdTask.Id}", updateDto, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // Act + Assert: GET UPDATED
        var updatedGetResponse = await client.GetAsync($"/api/OperationTasks/{createdTask.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, updatedGetResponse.StatusCode);

        var updatedTask = await updatedGetResponse.Content.ReadFromJsonAsync<OperationTaskDto>(TestContext.Current.CancellationToken);

        Assert.NotNull(updatedTask);

        Assert.Equal(updateDto.Title, updatedTask.Title);

        Assert.Equal(updateDto.Description, updatedTask.Description);

        Assert.True(updatedTask.IsCompleted);
        Assert.NotNull(updatedTask.CompletedAt);

        Assert.NotEqual(originalRowVersion, updatedTask.RowVersion);

        // Act + Assert: DELETE
        var deleteResponse = await client.DeleteAsync($"/api/OperationTasks/{createdTask.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Act + Assert: GET DELETED
        var deletedGetResponse = await client.GetAsync($"/api/OperationTasks/{createdTask.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, deletedGetResponse.StatusCode);
    }
}