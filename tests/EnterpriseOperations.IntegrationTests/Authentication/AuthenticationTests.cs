using EnterpriseOperations.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOperations.IntegrationTests.Authentication;

[Collection(IntegrationTestCollection.Name)]
public class AuthenticationTests
{
    private readonly MsSqlContainerFixture _sqlFixture;

    public AuthenticationTests(MsSqlContainerFixture sqlFixture)
    {
        _sqlFixture = sqlFixture;
    }

    [Fact]
    public async Task Login_WithValidCredentials_AllowsAccessToProtectedEndpoint()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory(_sqlFixture.ConnectionString);

        const string email = "integration-user@example.com";

        const string password = "Integration123!";

        await factory.CreateUserAsync(email, password);

        var client = factory.CreateClient();

        var loginRequest = new
        {
            email,
            password
        };

        // Act - Login
        var loginResponse = await client.PostAsJsonAsync("/login?useCookies=false", loginRequest);

        // Assert Login
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(tokenResponse);
        Assert.False(string.IsNullOrWhiteSpace(tokenResponse.AccessToken));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        // Act - protected request
        var protectedResponse = await client.GetAsync("/api/OperationTasks");

        // Assert protected request
        Assert.Equal(HttpStatusCode.OK, protectedResponse.StatusCode);
    }
}
