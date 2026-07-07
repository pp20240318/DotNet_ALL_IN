using Microsoft.AspNetCore.Mvc.Testing;

namespace WatchShop.Tests;

[CollectionDefinition("AdminApi")]
public class AdminApiCollection : ICollectionFixture<WebApplicationFactory<WatchShop.Admin.Api.AdminApiEntryPoint>>
{
}
