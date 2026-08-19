using EnterpriseOperations.Application.DTOs;
using EnterpriseOperations.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.IntegrationTests.Authentication;

[Collection(IntegrationTestCollection.Name)]
public class RoleAuthorizationTests
{
    private readonly MsSqlContainerFixture _sqlFixture;

    public RoleAuthorizationTests(MsSqlContainerFixture sqlFixture)
    {
        _sqlFixture = sqlFixture;
    }

    [Fact]
    public async Task DeleteOperationTask_AsOperator_ReturnsForbidden()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory(_sqlFixture.ConnectionString);

        const string email = "operator-integration@example.com";

        const string password = "Operator123!";

        await factory.CreateUserWithRoleAsync(email, password, "Operator");

        var client = factory.CreateClient();

        var accessToken = await factory.LoginAsync(client, email, password);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await client.DeleteAsync("/api/OperationTasks/1");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteOperationTask_AsAdmin_ReturnsNoContent()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory(_sqlFixture.ConnectionString);

        const string email = "admin-integration@example.com";

        const string password = "Admin123!";

        await factory.CreateUserWithRoleAsync(email, password, "Admin");

        var client = factory.CreateClient();

        var accessToken = await factory.LoginAsync(client, email, password);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createDto = new CreateOperationTaskDto
        {
            Title = "Task for admin delete test",
            Description = "Created by the integration test."
        };

        var createResponse = await client.PostAsJsonAsync("/api/OperationTasks", createDto);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdTask = await createResponse.Content.ReadFromJsonAsync<OperationTaskDto>();

        Assert.NotNull(createdTask);

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/OperationTasks/{createdTask.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
