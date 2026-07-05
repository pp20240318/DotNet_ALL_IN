using WatchShop.Admin.Api.Extensions;
using WatchShop.Admin.Api.Middleware;
using WatchShop.Application;
using WatchShop.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerWithJwt();

var app = builder.Build();

app.UseExceptionHandling();
app.UseDatabaseInitializer();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
