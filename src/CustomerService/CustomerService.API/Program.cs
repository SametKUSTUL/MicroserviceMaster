using CustomerService.API.Extensions;
using CustomerService.API.Middleware;
using CustomerService.Infrastructure.Data;
using Observability;
using Security;
using Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability("CustomerService");

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCustomerServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
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
