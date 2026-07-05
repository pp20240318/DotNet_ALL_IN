using Serilog;
using WatchShop.Admin.Api.Extensions;
using WatchShop.Admin.Api.Middleware;
using WatchShop.Application;
using WatchShop.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure();
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddWatchShopHealthChecks(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerWithJwt();

var app = builder.Build();

app.UseExceptionHandling();
app.UseDatabaseInitializer();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapWatchShopHealthChecks();
app.MapControllers();

app.Run();
