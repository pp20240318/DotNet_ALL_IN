using Serilog;
using WatchShop.Application;
using WatchShop.Infrastructure;
using WatchShop.Store.Api.Extensions;
using WatchShop.Store.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration).WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddWatchShopApiVersioning();
builder.Services.AddStoreSwagger();
builder.Services.AddStoreRateLimiting();
builder.Services.AddStoreHealthChecks(builder.Configuration);
builder.Services.AddWatchShopOpenTelemetry("WatchShop.Store.Api");

builder.Services.AddCors(options =>
{
    options.AddPolicy("store", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddStoreJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandling();
app.UseDatabaseInitializer();
app.UseSerilogRequestLogging();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("store");
app.UseAuthentication();
app.UseAuthorization();
app.MapStoreHealthChecks();
app.MapWatchShopMetrics();
app.MapControllers();

app.Run();
