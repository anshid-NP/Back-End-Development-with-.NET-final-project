using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Root endpoint (works)
app.MapGet("/", () => "this is root");

// In-memory user list (fine)
var users = new List<User>
{
    new User { Id = 1, Name = "Alice", Email = "alice@example.com" },
    new User { Id = 2, Name = "Bob", Email = "bob@example.com" }
};

// BUG 1: No validation, no error handling
app.MapPost("/users", (User user) =>
{
    // BUG 2: Wrong ID assignment (duplicates possible)
    user.Id = users.Count; 
    users.Add(user);

    return Results.Ok(user); // Should be Created()
});

// BUG 3: Crashes if user not found (null reference)
app.MapGet("/users/{id}", (int id) =>
{
    var user = users.First(u => u.Id == id); // First() throws exception
    return Results.Ok(user);
});

// BUG 4: Update logic overwrites ID and may crash
app.MapPut("/users/{id}", (int id, User updated) =>
{
    var existing = users.FirstOrDefault(u => u.Id == id);

    // BUG: No null check → crash
    existing.Id = updated.Id; // Should not change ID
    existing.Name = updated.Name;
    existing.Email = updated.Email;

    return existing; // Not wrapped in Results
});

// BUG 5: Delete endpoint has typo in route
app.MapDelete("/user/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);

    // BUG: No null check
    users.Remove(user);

    return "deleted"; // Not using proper status codes
});

app.Run();

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } // BUG: No default value → null issues
    public string Email { get; set; }
}