using Microsoft.EntityFrameworkCore;
using Observability;
using ProductService.API.Extensions;
using ProductService.API.Middleware;
using ProductService.Infrastructure.Data;
using Security;
using Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability("ProductService");

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddProductServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
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
