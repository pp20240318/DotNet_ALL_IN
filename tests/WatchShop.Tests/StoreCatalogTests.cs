using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WatchShop.Tests;

public class StoreCatalogTests : IClassFixture<WebApplicationFactory<WatchShop.Store.Api.StoreApiEntryPoint>>
{
    private readonly HttpClient _client;

    public StoreCatalogTests(WebApplicationFactory<WatchShop.Store.Api.StoreApiEntryPoint> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCatalogProducts_ReturnsSuccessEnvelope()
    {
        var response = await _client.GetAsync("/catalog/products");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"code\":0", json);
    }

    [Fact]
    public async Task Store_Api_Version_Endpoint_Should_Return_V1()
    {
        var response = await _client.GetAsync("/api/version");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("1.0", json);
        Assert.Contains("WatchShop.Store.Api", json);
    }
}
