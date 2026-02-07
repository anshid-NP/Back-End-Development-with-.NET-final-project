using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Root endpoint
app.MapGet("/", () => "this is root");

// In-memory user store
var users = new List<User>
{
    new User { Id = 1, Name = "Alice", Email = "alice@example.com" },
    new User { Id = 2, Name = "Bob", Email = "bob@example.com" }
};

// GET all users
app.MapGet("/users", () =>
{
    return Results.Ok(users);
});

// GET user by ID
app.MapGet("/users/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
});

// POST create new user
app.MapPost("/users", (User newUser) =>
{
    newUser.Id = users.Count == 0 ? 1 : users.Max(u => u.Id) + 1;
    users.Add(newUser);
    return Results.Created($"/users/{newUser.Id}", newUser);
});

// PUT update existing user
app.MapPut("/users/{id:int}", (int id, User updatedUser) =>
{
    var existing = users.FirstOrDefault(u => u.Id == id);
    if (existing is null)
        return Results.NotFound();

    existing.Name = updatedUser.Name;
    existing.Email = updatedUser.Email;

    return Results.Ok(existing);
});

// DELETE user by ID
app.MapDelete("/users/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null)
        return Results.NotFound();

    users.Remove(user);
    return Results.NoContent();
});

app.Run();

// User model
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}