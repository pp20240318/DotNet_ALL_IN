using WatchShop.Infrastructure.Persistence;

namespace WatchShop.Store.Api.Extensions;

public static class DatabaseInitializerExtensions
{
    public static WebApplication UseDatabaseInitializer(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<DbInitializer>().Initialize();
        return app;
    }
}
