using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SupportTickets.Application.DTOs;
using SupportTickets.Domain.Enums;
using Xunit;

namespace SupportTickets.IntegrationTests;

public class TicketDataIsolationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TicketDataIsolationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient client, AuthResponse auth)> RegisterAndLoginAsync(string emailPrefix)
    {
        var client = _factory.CreateClient();
        var email = $"{emailPrefix}_{Guid.NewGuid():N}@test.com";
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password1!", emailPrefix), JsonTestOptions.Options);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonTestOptions.Options);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        return (client, auth);
    }

    [Fact]
    public async Task CustomerA_CannotGetCustomerBsTicket_ByIncrementingId()
    {
        var (clientA, _) = await RegisterAndLoginAsync("customerA");
        var (clientB, _) = await RegisterAndLoginAsync("customerB");

        var createResponse = await clientB.PostAsJsonAsync("/api/tickets", new CreateTicketRequest("B's private issue", "Sensitive details", TicketPriority.Medium), JsonTestOptions.Options);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var bTicket = await createResponse.Content.ReadFromJsonAsync<TicketDto>(JsonTestOptions.Options);

        // Customer A tries to fetch B's ticket directly by id
        var getResponse = await clientA.GetAsync($"/api/tickets/{bTicket!.Id}");

        getResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CustomerA_CannotUpdateCustomerBsTicket_ByIncrementingId()
    {
        var (clientA, _) = await RegisterAndLoginAsync("updA");
        var (clientB, _) = await RegisterAndLoginAsync("updB");

        var createResponse = await clientB.PostAsJsonAsync("/api/tickets", new CreateTicketRequest("B ticket to protect", "Details", TicketPriority.Low), JsonTestOptions.Options);
        var bTicket = await createResponse.Content.ReadFromJsonAsync<TicketDto>(JsonTestOptions.Options);

        var updateResponse = await clientA.PutAsJsonAsync($"/api/tickets/{bTicket!.Id}", new UpdateTicketRequest("Hacked title", null, null, null, null), JsonTestOptions.Options);

        updateResponse.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CustomerA_CannotSeeCustomerBsTicket_InListing()
    {
        var (clientA, _) = await RegisterAndLoginAsync("listA");
        var (clientB, _) = await RegisterAndLoginAsync("listB");

        var uniqueTitle = $"UniqueTicketFor_B_{Guid.NewGuid():N}";
        await clientB.PostAsJsonAsync("/api/tickets", new CreateTicketRequest(uniqueTitle, "Details", TicketPriority.Low), JsonTestOptions.Options);

        var listResponse = await clientA.GetAsync("/api/tickets?pageSize=100");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await listResponse.Content.ReadAsStringAsync();
        content.Should().NotContain(uniqueTitle);
    }

    [Fact]
    public async Task Unauthenticated_CannotAccessTickets()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/tickets");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
