using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagementAPI.Models;
using static UserManagementAPI.Models.UserModel;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("AppDb"));

builder.Services.AddIdentityCore<IdentityUser>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddApiEndpoints();

builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorizationBuilder();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapIdentityApi<IdentityUser>();

app.MapGet("/test", (ClaimsPrincipal user) => $"Hello {user.Identity!.Name}").RequireAuthorization();

// Endpoint to list all users
app.MapGet("/users", async (UserManager<IdentityUser> userManager) =>
{
    var users = await userManager.Users.ToListAsync();
    return Results.Ok(users);
}).RequireAuthorization();

// Endpoint to edit user details
app.MapPut("/users/{id}", async (string id, UserManager<IdentityUser> userManager, [FromBody] IdentityUser updatedUser) =>
{
    var user = await userManager.FindByIdAsync(id);
    if (user == null) return Results.NotFound("User not found");

    user.UserName = updatedUser.UserName ?? user.UserName;
    user.Email = updatedUser.Email ?? user.Email;
    // Update other properties as needed

    var result = await userManager.UpdateAsync(user);
    if (result.Succeeded) return Results.Ok("User updated successfully");

    return Results.BadRequest(result.Errors);
}).RequireAuthorization();

// Endpoint to delete a user
app.MapDelete("/users/{id}", async (string id, UserManager<IdentityUser> userManager) =>
{
    var user = await userManager.FindByIdAsync(id);
    if (user == null) return Results.NotFound("User not found");

    var result = await userManager.DeleteAsync(user);
    if (result.Succeeded) return Results.Ok("User deleted successfully");

    return Results.BadRequest(result.Errors);
}).RequireAuthorization();

// Endpoint to change user's password
app.MapPost("/users/change-password/{id}", async (string id, [FromBody] ChangePasswordModel model, UserManager<IdentityUser> userManager) =>
{
    var user = await userManager.FindByIdAsync(id);
    if (user == null) return Results.NotFound("User not found");

    var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
    if (result.Succeeded) return Results.Ok("Password changed successfully");

    return Results.BadRequest(result.Errors);
}).RequireAuthorization();



app.Run();

class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
