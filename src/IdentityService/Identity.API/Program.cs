using Identity.API.Services;
using Observability;
using Security;
using Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddObservability("IdentityService");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Identity API", Version = "v1" });
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() 
                  ?? throw new InvalidOperationException("JwtSettings configuration is missing");

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<JwtTokenGenerator>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.MapControllers();

try
{
    app.Run();
}
finally
{
    Serilog.Log.CloseAndFlush();
}
