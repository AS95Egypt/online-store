using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace OnlineStore.Api.IntegrationTests;

public sealed class ApiAccessTests : IClassFixture<OnlineStoreApiFactory>
{
    private readonly OnlineStoreApiFactory _factory;

    public ApiAccessTests(OnlineStoreApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthy()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/health");
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", body?.Status);
    }

    [Fact]
    public async Task GetAdminBrands_WithoutToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/admin/brands");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAdminBrands_AsCustomer_ReturnsForbidden()
    {
        using var client = _factory.CreateAuthenticatedClient("CUSTOMER");

        var response = await client.GetAsync("/api/admin/brands");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetOrders_AsAdmin_ReturnsForbidden()
    {
        using var client = _factory.CreateAuthenticatedClient("ADMIN");

        var response = await client.GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record HealthResponse(string Status);
}
