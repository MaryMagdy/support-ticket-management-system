using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SupportTickets.Application.DTOs;
using Xunit;

namespace SupportTickets.IntegrationTests;

public class AuthFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_Then_Login_ReturnsTokens()
    {
        var client = _factory.CreateClient();

        var email = $"newuser_{Guid.NewGuid():N}@test.com";
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1!", "New User"), JsonTestOptions.Options);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var registerBody = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonTestOptions.Options);
        registerBody.Should().NotBeNull();
        registerBody!.AccessToken.Should().NotBeNullOrEmpty();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password1!"), JsonTestOptions.Options);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonTestOptions.Options);
        loginBody!.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = _factory.CreateClient();
        var email = $"wrongpass_{Guid.NewGuid():N}@test.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1!", "User"), JsonTestOptions.Options);

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPassword!"), JsonTestOptions.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_RotatesToken_AndOldTokenBecomesInvalid()
    {
        var client = _factory.CreateClient();
        var email = $"refresh_{Guid.NewGuid():N}@test.com";
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1!", "User"), JsonTestOptions.Options);
        var body = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonTestOptions.Options);

        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(body!.RefreshToken), JsonTestOptions.Options);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newBody = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonTestOptions.Options);
        newBody!.RefreshToken.Should().NotBe(body.RefreshToken);

        // Reusing the old (now-revoked) refresh token should fail
        var reuseResponse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(body.RefreshToken), JsonTestOptions.Options);
        reuseResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var client = _factory.CreateClient();
        var email = $"dup_{Guid.NewGuid():N}@test.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1!", "User"), JsonTestOptions.Options);

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1!", "User"), JsonTestOptions.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
