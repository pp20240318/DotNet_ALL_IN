using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using WatchShop.Application.Common;
using WatchShop.Application.Features.Store.Dtos;

namespace WatchShop.Tests;

public class StoreCatalogTests : IClassFixture<WebApplicationFactory<WatchShop.Store.Api.StoreApiEntryPoint>>
{
    private readonly WebApplicationFactory<WatchShop.Store.Api.StoreApiEntryPoint> _factory;
    private readonly HttpClient _client;

    public StoreCatalogTests(WebApplicationFactory<WatchShop.Store.Api.StoreApiEntryPoint> factory)
    {
        _factory = factory;
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

    [Fact]
    public async Task Customer_Can_Login_And_Get_Cart()
    {
        var client = await CreateStoreClientAsync("demo", "Demo@123");
        var cartResponse = await client.GetAsync("/store/cart");
        cartResponse.EnsureSuccessStatusCode();
        var envelope = await cartResponse.Content.ReadFromJsonAsync<ApiResult<List<CartItemResponse>>>();
        Assert.NotNull(envelope?.Data);
    }

    private async Task<HttpClient> CreateStoreClientAsync(string username, string password)
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/store/auth/login", new { username, password });
        loginResponse.EnsureSuccessStatusCode();
        var envelope = await loginResponse.Content.ReadFromJsonAsync<ApiResult<CustomerLoginResponse>>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", envelope!.Data!.Token);
        return client;
    }
}
