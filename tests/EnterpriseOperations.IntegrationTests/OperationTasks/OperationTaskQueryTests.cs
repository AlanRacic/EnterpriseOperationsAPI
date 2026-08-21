using EnterpriseOperations.Application.DTOs;
using EnterpriseOperations.Application.Models;
using EnterpriseOperations.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.IntegrationTests.OperationTasks;

[Collection(IntegrationTestCollection.Name)]
public class OperationTaskQueryTests
{
    private readonly MsSqlContainerFixture _sqlFixture;

    public OperationTaskQueryTests(MsSqlContainerFixture sqlFixture)
    {
        _sqlFixture = sqlFixture;
    }

    [Fact]
    public async Task GetPagedOperationTasks_WithFilteringSortingAndPaging_ReturnsExpectedResult()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory(_sqlFixture.ConnectionString);

        var client = factory.CreateClient();

        var uniqueId = Guid.NewGuid().ToString("N");

        var email = $"query-admin-{uniqueId}@example.com";

        const string password = "Admin123!";

        await factory.CreateUserWithRoleAsync(email, password, "Admin");

        var accessToken = await factory.LoginAsync(client, email, password);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var searchMarker = $"Report-{uniqueId}";

        var tasks = new[]
        {
            new CreateOperationTaskDto
            {
                Title = $"{searchMarker} Alpha",
                Description = "First report task."
            },
        new CreateOperationTaskDto
            {
                Title = $"{searchMarker} Beta",
                Description = "Second report task."
            },
        new CreateOperationTaskDto
            {
                Title = $"Supplier-{uniqueId}",
                Description = "Unrelated supplier task."
            }
        };

        foreach (var task in tasks)
        {
            var createResponse = await client.PostAsJsonAsync("/api/OperationTasks", task);

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        }

        var searchTerm = Uri.EscapeDataString(searchMarker);

        // Act
        var response = await client.GetAsync(
                $"/api/OperationTasks/paged" +
                $"?pageNumber=1" +
                $"&pageSize=2" +
                $"&isCompleted=false" +
                $"&searchTerm={searchTerm}" +
                $"&sortBy=title" +
                $"&sortDirection=asc");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<OperationTaskDto>>();

        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.TotalCount);

        var items = result.Items.ToList();

        Assert.Equal(2, items.Count);

        Assert.All(items, item =>
            {
                Assert.Contains(searchMarker, item.Title);
                Assert.False(item.IsCompleted);
            });

        Assert.True(string.Compare(
                items[0].Title,
                items[1].Title,
                StringComparison.Ordinal) <= 0);
    }
}
