using Microsoft.EntityFrameworkCore;
using OrderService.API.Extensions;
using OrderService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

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

app.UseAuthorization();
app.MapControllers();

app.Run();
