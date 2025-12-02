using Microsoft.EntityFrameworkCore;
using Observability;
using OrderService.API.Extensions;
using OrderService.API.Middleware;
using OrderService.Infrastructure.Data;
using Security;
using Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability("OrderService");

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddOrderServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
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
