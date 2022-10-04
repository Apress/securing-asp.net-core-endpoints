using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("webApi", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://localhost:7239");
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.SignInScheme = "Cookies";
    options.Authority = "https://localhost:5001/";
    options.RequireHttpsMetadata = true;
    options.ClientId = "aspNetCoreAuth";
    options.ClientSecret = "some_secret";
    options.ResponseType = "code";
    options.UsePkce = true;
    options.SaveTokens = true;
    options.CallbackPath = "/signin-oidc";
    options.RequireHttpsMetadata = false;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Events.OnUserInformationReceived = context =>
    {
        try
        {
            var roleElement = context.User.RootElement.GetProperty("role");

            var claims = new List<Claim>();
            if (roleElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                foreach (var r in roleElement.EnumerateArray())
                    claims.Add(new Claim(ClaimTypes.Role, r.GetString()));
            else
                claims.Add(new Claim(ClaimTypes.Role, roleElement.GetString()));

            var id = context.Principal.Identity as ClaimsIdentity;
            id.AddClaims(claims);
        }
        catch
        {
            // No roles exist for the user
        }

        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();