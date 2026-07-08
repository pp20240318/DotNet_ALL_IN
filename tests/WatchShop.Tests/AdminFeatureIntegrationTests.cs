using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Common;
using WatchShop.Application.Dtos.Auth;
using WatchShop.Application.Features.Customers;
using WatchShop.Application.Features.Rbac;
using WatchShop.Domain.Entities;
using WatchShop.Domain.Enums;

namespace WatchShop.Tests;

[Collection("AdminApi")]
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

    [Fact]
    public async Task Admin_Can_Export_Products_Csv()
    {
        var client = await CreateAuthenticatedClientAsync("admin", "Admin@123");
        var response = await client.GetAsync("/products/export");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("Name", csv);
    }

    [Fact]
    public async Task Refresh_Token_Should_Issue_New_Access_Token()
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/auth/login", new { username = "admin", password = "Admin@123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<ApiResult<LoginResponse>>();
        Assert.False(string.IsNullOrWhiteSpace(login?.Data?.RefreshToken));

        var refreshResponse = await client.PostAsJsonAsync("/auth/refresh", new { refreshToken = login!.Data!.RefreshToken });
        refreshResponse.EnsureSuccessStatusCode();
        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<ApiResult<LoginResponse>>();
        Assert.False(string.IsNullOrWhiteSpace(refreshed?.Data?.Token));
        Assert.NotEqual(login.Data.RefreshToken, refreshed!.Data!.RefreshToken);
    }

    [Fact]
    public async Task Admin_Can_Create_New_Admin()
    {
        var adminClient = await CreateAuthenticatedClientAsync("admin", "Admin@123");
        var username = $"test_{Guid.NewGuid():N}"[..12];

        var createResponse = await adminClient.PostAsJsonAsync("/roles/admins", new
        {
            username,
            password = "Test@12345",
            displayName = "测试管理员",
            roles = new[] { "Viewer" }
        });
        createResponse.EnsureSuccessStatusCode();

        var listResponse = await adminClient.GetAsync("/roles/admins");
        var list = await listResponse.Content.ReadFromJsonAsync<ApiResult<List<AdminRoleResponse>>>();
        Assert.Contains(list?.Data ?? [], x => x.Username == username);
    }

    [Fact]
    public async Task Admin_Can_Update_Admin_DisplayName_And_Disable()
    {
        var adminClient = await CreateAuthenticatedClientAsync("admin", "Admin@123");
        var username = $"upd_{Guid.NewGuid():N}"[..12];

        var createResponse = await adminClient.PostAsJsonAsync("/roles/admins", new
        {
            username,
            password = "Test@12345",
            displayName = "待更新",
            roles = new[] { "Viewer" }
        });
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResult<Dictionary<string, long>>>();
        var adminId = created!.Data!["id"];

        var updateResponse = await adminClient.PutAsJsonAsync($"/roles/admins/{adminId}", new
        {
            displayName = "已更新",
            isEnabled = false
        });
        updateResponse.EnsureSuccessStatusCode();

        var listResponse = await adminClient.GetAsync("/roles/admins");
        var list = await listResponse.Content.ReadFromJsonAsync<ApiResult<List<AdminRoleResponse>>>();
        var updated = list?.Data?.FirstOrDefault(x => x.Id == adminId);
        Assert.NotNull(updated);
        Assert.Equal("已更新", updated!.DisplayName);
        Assert.False(updated.IsEnabled);
    }

    [Fact]
    public async Task Admin_Can_List_Customers()
    {
        var adminClient = await CreateAuthenticatedClientAsync("admin", "Admin@123");
        var response = await adminClient.GetAsync("/customers?page=1&pageSize=10");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"code\":0", json);
        Assert.Contains("demo", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Admin_Can_Update_Customer_Enable_State()
    {
        var adminClient = await CreateAuthenticatedClientAsync("admin", "Admin@123");
        var listResponse = await adminClient.GetAsync("/customers?page=1&pageSize=50");
        var list = await listResponse.Content.ReadFromJsonAsync<ApiResult<PagedResult<CustomerAdminResponse>>>();
        var demo = list?.Data?.Items.FirstOrDefault(x => x.Username == "demo");
        Assert.NotNull(demo);

        var updateResponse = await adminClient.PutAsJsonAsync($"/customers/{demo!.Id}", new { isEnabled = false });
        updateResponse.EnsureSuccessStatusCode();

        listResponse = await adminClient.GetAsync("/customers?page=1&pageSize=50");
        list = await listResponse.Content.ReadFromJsonAsync<ApiResult<PagedResult<CustomerAdminResponse>>>();
        var updated = list?.Data?.Items.FirstOrDefault(x => x.Id == demo.Id);
        Assert.NotNull(updated);
        Assert.False(updated!.IsEnabled);

        await adminClient.PutAsJsonAsync($"/customers/{demo.Id}", new { isEnabled = true });
    }

    [Fact]
    public async Task Admin_Can_Refund_Paid_Order()
    {
        long orderId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            await EnsureCatalogSeedAsync(db);

            orderId = await orderService.CreateDemoOrderAsync();
            await db.Updateable<ShopOrder>()
                .SetColumns(x => x.Status == OrderStatus.Paid)
                .SetColumns(x => x.PaidAt == DateTime.UtcNow)
                .Where(x => x.Id == orderId)
                .ExecuteCommandAsync();
        }

        var adminClient = await CreateAuthenticatedClientAsync("admin", "Admin@123");
        var refundResponse = await adminClient.PostAsync($"/orders/{orderId}/refund", null);
        refundResponse.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
            var after = await db.Queryable<ShopOrder>().InSingleAsync(orderId);
            Assert.Equal(OrderStatus.Refunded, after!.Status);
            Assert.NotNull(after.RefundedAt);
        }

        var detailResponse = await adminClient.GetAsync($"/orders/{orderId}");
        detailResponse.EnsureSuccessStatusCode();
        var detail = await detailResponse.Content.ReadFromJsonAsync<ApiResult<OrderDetailResponse>>();
        Assert.Equal(OrderStatus.Refunded, detail?.Data?.Status);
    }

    private static async Task EnsureCatalogSeedAsync(ISqlSugarClient db)
    {
        if (await db.Queryable<ProductSku>().Where(x => !x.IsDeleted).AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var brandId = SnowFlakeSingle.Instance.NextId();
        await db.Insertable(new Brand
        {
            Id = brandId,
            Name = "TestBrand",
            Description = "test",
            SortOrder = 1,
            IsEnabled = true,
            IsDeleted = false,
            Version = 0,
            CreatedAt = now,
            UpdatedAt = now,
        }).ExecuteCommandAsync();

        var categoryId = SnowFlakeSingle.Instance.NextId();
        await db.Insertable(new Category
        {
            Id = categoryId,
            Name = "TestCategory",
            SortOrder = 1,
            IsEnabled = true,
            IsDeleted = false,
            Version = 0,
            CreatedAt = now,
            UpdatedAt = now,
        }).ExecuteCommandAsync();

        var productId = SnowFlakeSingle.Instance.NextId();
        await db.Insertable(new Product
        {
            Id = productId,
            Name = "Refund Test Product",
            BrandId = brandId,
            CategoryId = categoryId,
            Description = "test",
            Price = 100m,
            Status = ProductStatus.OnSale,
            IsDeleted = false,
            Version = 0,
            CreatedAt = now,
            UpdatedAt = now,
        }).ExecuteCommandAsync();

        await db.Insertable(new ProductSku
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            ProductId = productId,
            SkuCode = "REF-001",
            SpecJson = "{}",
            Price = 100m,
            Stock = 10,
            IsEnabled = true,
            IsDeleted = false,
            Version = 0,
            CreatedAt = now,
            UpdatedAt = now,
        }).ExecuteCommandAsync();
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
