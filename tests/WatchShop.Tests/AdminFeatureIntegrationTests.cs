using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using WatchShop.Application.Common;
using WatchShop.Application.Dtos.Auth;
using WatchShop.Application.Features.Rbac;

namespace WatchShop.Tests;

public class AdminFeatureIntegrationTests : IClassFixture<WebApplicationFactory<WatchShop.Admin.Api.AdminApiEntryPoint>>
{
    private readonly WebApplicationFactory<WatchShop.Admin.Api.AdminApiEntryPoint> _factory;

    public AdminFeatureIntegrationTests(WebApplicationFactory<WatchShop.Admin.Api.AdminApiEntryPoint> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Api_Version_Endpoint_Should_Return_V1()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/version");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("1.0", json);
        Assert.Contains("WatchShop.Admin.Api", json);
    }

    [Fact]
    public async Task Admin_Can_Export_Orders_Csv()
    {
        var client = await CreateAuthenticatedClientAsync("admin", "Admin@123");
        var response = await client.GetAsync("/orders/export");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("csv", response.Content.Headers.ContentType?.MediaType);

        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("OrderNo", csv);
    }

    [Fact]
    public async Task Admin_Can_Assign_Roles_And_Permissions_Update()
    {
        var adminClient = await CreateAuthenticatedClientAsync("admin", "Admin@123");
        var adminsResponse = await adminClient.GetAsync("/roles/admins");
        adminsResponse.EnsureSuccessStatusCode();
        var adminsEnvelope = await adminsResponse.Content.ReadFromJsonAsync<ApiResult<List<AdminRoleResponse>>>();
        var viewer = adminsEnvelope?.Data?.FirstOrDefault(x => x.Username == "viewer");
        Assert.NotNull(viewer);

        try
        {
            var assignResponse = await adminClient.PutAsJsonAsync(
                $"/roles/admins/{viewer!.Id}/roles",
                new { roles = new[] { "Operator" } });
            assignResponse.EnsureSuccessStatusCode();

            var viewerClient = await CreateAuthenticatedClientAsync("viewer", "Viewer@123");
            var profileResponse = await viewerClient.GetAsync("/auth/me");
            profileResponse.EnsureSuccessStatusCode();
            var profile = await profileResponse.Content.ReadFromJsonAsync<ApiResult<AdminProfileResponse>>();
            Assert.Contains("product:write", profile?.Data?.Permissions ?? []);
            Assert.Contains("Operator", profile?.Data?.Roles ?? []);
        }
        finally
        {
            await adminClient.PutAsJsonAsync(
                $"/roles/admins/{viewer!.Id}/roles",
                new { roles = new[] { "Viewer" } });
        }
    }

    [Fact]
    public async Task Assign_Invalid_Role_Should_Return_BadRequest()
    {
        var adminClient = await CreateAuthenticatedClientAsync("admin", "Admin@123");
        var adminsResponse = await adminClient.GetAsync("/roles/admins");
        var adminsEnvelope = await adminsResponse.Content.ReadFromJsonAsync<ApiResult<List<AdminRoleResponse>>>();
        var viewer = adminsEnvelope?.Data?.First(x => x.Username == "viewer");

        var response = await adminClient.PutAsJsonAsync(
            $"/roles/admins/{viewer!.Id}/roles",
            new { roles = new[] { "NotExists" } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string username, string password)
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/auth/login", new { username, password });
        loginResponse.EnsureSuccessStatusCode();

        var envelope = await loginResponse.Content.ReadFromJsonAsync<ApiResult<LoginResponse>>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", envelope!.Data!.Token);
        return client;
    }
}
