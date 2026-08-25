using InventorySystem.API.Configuration;
using InventorySystem.API.Middleware;
using InventorySystem.Application;
using InventorySystem.Infrastructure;
using InventorySystem.API.Services;
using InventorySystem.Application.Common.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddFrontendCors(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<InventorySystem.Application.Interfaces.ICurrentUserService, HttpCurrentUserService>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole(ApplicationRoles.Admin));
    options.AddPolicy(AuthorizationPolicies.EditorOrAdmin,
        policy => policy.RequireRole(ApplicationRoles.Admin, ApplicationRoles.Editor));
});
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

await app.Services.InitializeDatabaseAsync(builder.Configuration);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors(CorsConfiguration.PolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
