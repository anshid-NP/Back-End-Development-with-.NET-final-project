using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserManagementApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services if needed (e.g., logging is already configured by default)
builder.Services.AddLogging();

var app = builder.Build();

// MIDDLEWARE PIPELINE ORDER:
// 1. Error handling
// 2. Authentication
// 3. Logging

app.UseErrorHandling();
app.UseTokenAuthentication();
app.UseRequestResponseLogging();

// Example health endpoint (no auth required in middleware)
app.MapGet("/health", () => Results.Ok(new { status = "OK" }));

// Example endpoint that will require a valid token
app.MapGet("/users", () =>
{
    var users = new[]
    {
        new { Id = 1, Name = "Alice" },
        new { Id = 2, Name = "Bob" }
    };

    return Results.Ok(users);
});

// Endpoint to simulate an exception for testing error handling
app.MapGet("/crash", () =>
{
    throw new Exception("Test exception");
});

app.Run();