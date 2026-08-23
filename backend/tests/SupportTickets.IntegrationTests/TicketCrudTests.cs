using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SupportTickets.Application.DTOs;
using SupportTickets.Domain.Enums;
using Xunit;

namespace SupportTickets.IntegrationTests;

public class TicketCrudTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TicketCrudTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> AuthenticatedClientAsync(string emailPrefix)
    {
        var client = _factory.CreateClient();
        var email = $"{emailPrefix}_{Guid.NewGuid():N}@test.com";
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1!", emailPrefix), JsonTestOptions.Options);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonTestOptions.Options);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        return client;
    }

    [Fact]
    public async Task Customer_CanCreateAndRetrieveOwnTicket()
    {
        var client = await AuthenticatedClientAsync("crud1");

        var createResponse = await client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest("My issue", "Something broke", TicketPriority.High), JsonTestOptions.Options);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<TicketDto>(JsonTestOptions.Options);
        created!.Status.Should().Be(TicketStatus.Open);

        var getResponse = await client.GetAsync($"/api/tickets/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Customer_CannotDeleteTicket()
    {
        var client = await AuthenticatedClientAsync("crud2");
        var createResponse = await client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest("To delete", "Details", TicketPriority.Low), JsonTestOptions.Options);
        var created = await createResponse.Content.ReadFromJsonAsync<TicketDto>(JsonTestOptions.Options);

        var deleteResponse = await client.DeleteAsync($"/api/tickets/{created!.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTicket_WithMissingTitle_Returns400()
    {
        var client = await AuthenticatedClientAsync("crud3");

        var response = await client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest("", "Details", TicketPriority.Low), JsonTestOptions.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Customer_AddComment_ThenRetrievesIt()
    {
        var client = await AuthenticatedClientAsync("crud4");
        var createResponse = await client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest("Comment test", "Details", TicketPriority.Low), JsonTestOptions.Options);
        var created = await createResponse.Content.ReadFromJsonAsync<TicketDto>(JsonTestOptions.Options);

        var commentResponse = await client.PostAsJsonAsync($"/api/tickets/{created!.Id}/comments", new CreateCommentRequest("Any updates?"), JsonTestOptions.Options);
        commentResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await client.GetAsync($"/api/tickets/{created.Id}/comments");
        var comments = await listResponse.Content.ReadFromJsonAsync<List<CommentDto>>();
        comments.Should().ContainSingle(c => c.Text == "Any updates?");
    }
}
