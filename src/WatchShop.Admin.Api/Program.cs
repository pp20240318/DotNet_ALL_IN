using Serilog;
using WatchShop.Admin.Api.Extensions;
using WatchShop.Admin.Api.Middleware;
using WatchShop.Application;
using WatchShop.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWatchShopOpenTelemetry("WatchShop.Admin.Api");
builder.Services.AddAdminRateLimiting();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddWatchShopHealthChecks(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddWatchShopAuthorization();
builder.Services.AddWatchShopSignalR();
builder.Services.AddWatchShopHangfire(builder.Configuration);
builder.Services.AddSwaggerWithJwt();

var app = builder.Build();

app.UseExceptionHandling();
app.UseDatabaseInitializer();
app.UseSerilogRequestLogging();
app.UseAdminRateLimiting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapWatchShopHealthChecks();
app.MapWatchShopMetrics();
app.UseWatchShopHangfire();
app.MapWatchShopSignalR();
app.MapControllers().RequireRateLimiting("admin-fixed");

app.Run();

