using WatchShop.Infrastructure.Persistence;

namespace WatchShop.Admin.Api.Extensions;

public static class DatabaseInitializerExtensions
{
    public static WebApplication UseDatabaseInitializer(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        initializer.Initialize();
        return app;
    }
}
