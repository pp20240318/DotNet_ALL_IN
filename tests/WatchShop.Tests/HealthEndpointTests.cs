using Microsoft.AspNetCore.Mvc.Testing;

namespace WatchShop.Tests;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<WatchShop.Admin.Api.AdminApiEntryPoint>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<WatchShop.Admin.Api.AdminApiEntryPoint> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Endpoint_Should_Return_Success()
    {
        var response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }
}
