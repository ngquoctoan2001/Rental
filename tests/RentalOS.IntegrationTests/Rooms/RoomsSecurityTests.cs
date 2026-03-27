using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using RentalOS.IntegrationTests.Infrastructure;

namespace RentalOS.IntegrationTests.Rooms;

/// <summary>
/// Integration tests for protected room endpoints.
/// These verify that authentication is enforced correctly.
/// </summary>
public class RoomsSecurityTests : IClassFixture<RentalOsWebFactory>
{
    private readonly HttpClient _client;

    public RoomsSecurityTests(RentalOsWebFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRooms_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/rooms");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateRoom_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/rooms", new { name = "Test Room" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetInvoices_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/invoices");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomers_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/customers");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetStaff_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/staff");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetReports_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/reports/dashboard");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
