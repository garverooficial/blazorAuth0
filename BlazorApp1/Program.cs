using Auth0.AspNetCore.Authentication;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using BlazorApp1.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"];
    options.ClientId = builder.Configuration["Auth0:ClientId"];
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


// Registro simple: crea el Management API client con Client Credentials  
builder.Services.AddScoped<IManagementApiClient>(sp =>  
{  
    var cfg = sp.GetRequiredService<IConfiguration>();  
    var domain = cfg["Auth0:Domain"];  
    var audience = cfg["Auth0:Audience"] ?? $"https://{domain}/api/v2/";  
    var clientId = cfg["Auth0:ClientId"];  
    var clientSecret = cfg["Auth0:ClientSecret"];  
  
    if (string.IsNullOrWhiteSpace(domain) ||  
        string.IsNullOrWhiteSpace(clientId) ||  
        string.IsNullOrWhiteSpace(clientSecret))  
    {        throw new InvalidOperationException("Faltan Auth0:Domain, Auth0:ClientId o Auth0:ClientSecret en la configuración.");  
    }  
    var authClient = new AuthenticationApiClient(new Uri($"https://{domain}/"));  
    var token = authClient.GetTokenAsync(new ClientCredentialsTokenRequest  
    {  
        Audience = audience,  
        ClientId = clientId,  
        ClientSecret = clientSecret  
    }).GetAwaiter().GetResult();  
  
    return new ManagementApiClient(token.AccessToken, new Uri(audience));  
});

builder.Services.AddRadzenComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapGet("/Account/Login", async (HttpContext httpContext, string returnUrl = "/") =>
{
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
        .WithRedirectUri(returnUrl)
        .Build();

    await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

app.MapGet("/Account/Logout", async (HttpContext httpContext, string returnUrl = "/") =>
{
    var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
        .WithRedirectUri(returnUrl)
        .Build();

    await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseAuthentication();
app.UseAuthorization();

app.Run();