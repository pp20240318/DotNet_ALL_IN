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
}
