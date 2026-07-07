using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace WatchShop.Admin.Api.Extensions;

public static class ApiVersioningExtensions
{
    public static IServiceCollection AddWatchShopApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader("X-Api-Version"),
                    new QueryStringApiVersionReader("api-version"));
            })
            .AddMvc();

        return services;
    }
}
