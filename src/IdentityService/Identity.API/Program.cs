using Identity.API.Extensions;
using Identity.Infrastructure.Data;
using Observability;
using Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability("IdentityService");

builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

try
{
    app.Run();
}
finally
{
    Serilog.Log.CloseAndFlush();
}
