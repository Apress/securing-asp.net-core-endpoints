using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.Authority = "https://localhost:5001/";
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicAuth", policy =>
    {
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("AdminClaim", policy =>
    {
        policy.RequireClaim("admin");
    });

    options.AddPolicy("AdminOnly", policy =>
    {
        policy.Requirements.Add(new RoleRequirement("admin"));
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/private-healthcheck", () => "The application is running")
    .RequireAuthorization(policyNames: "BasicAuth");
app.MapGet("/public-healthcheck", () => "The application is running")
    .AllowAnonymous();

app.Run();

public class RoleRequirement : AuthorizationHandler<RoleRequirement>,
    IAuthorizationRequirement
{
    private readonly string requiredRole;

    public RoleRequirement(string requiredRole)
    {
        this.requiredRole = requiredRole;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        var roles = ((ClaimsIdentity)context.User.Identity).Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);

        if (roles.Contains(requiredRole))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}