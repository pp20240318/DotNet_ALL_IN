using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using WatchShop.Application.Common;
using WatchShop.Application.Dtos.Auth;

namespace WatchShop.Tests;

public class RbacIntegrationTests : IClassFixture<WebApplicationFactory<WatchShop.Admin.Api.AdminApiEntryPoint>>
{
    private readonly WebApplicationFactory<WatchShop.Admin.Api.AdminApiEntryPoint> _factory;

    public RbacIntegrationTests(WebApplicationFactory<WatchShop.Admin.Api.AdminApiEntryPoint> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Viewer_Can_Read_Products_But_Cannot_Write_Brand()
    {
        var client = await CreateAuthenticatedClientAsync("viewer", "Viewer@123");

        var readResponse = await client.GetAsync("/products?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);

        var writeResponse = await client.PostAsJsonAsync("/brands", new
        {
            name = "TestBrand",
            description = "rbac test",
            sortOrder = 99,
            isEnabled = true
        });
        Assert.Equal(HttpStatusCode.Forbidden, writeResponse.StatusCode);
    }

    [Fact]
    public async Task Operator_Login_Should_Include_Write_Permissions()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/login", new { username = "operator", password = "Operator@123" });
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<ApiResult<LoginResponse>>();
        Assert.NotNull(envelope?.Data);
        Assert.Contains("product:write", envelope.Data.Permissions);
        Assert.DoesNotContain("system:admin", envelope.Data.Permissions);
    }

    [Fact]
    public async Task Admin_Can_Query_Roles_Viewer_Cannot()
    {
        var adminClient = await CreateAuthenticatedClientAsync("admin", "Admin@123");
        var rolesResponse = await adminClient.GetAsync("/roles");
        Assert.Equal(HttpStatusCode.OK, rolesResponse.StatusCode);

        var viewerClient = await CreateAuthenticatedClientAsync("viewer", "Viewer@123");
        var forbiddenResponse = await viewerClient.GetAsync("/roles");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string username, string password)
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/auth/login", new { username, password });
        loginResponse.EnsureSuccessStatusCode();

        var envelope = await loginResponse.Content.ReadFromJsonAsync<ApiResult<LoginResponse>>();
        Assert.False(string.IsNullOrWhiteSpace(envelope?.Data?.Token));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", envelope!.Data!.Token);
        return client;
    }
}
