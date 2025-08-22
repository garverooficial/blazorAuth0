# BlazorAuth0 🔐

Este proyecto muestra paso a paso cómo conectar una aplicación **Blazor** con **Auth0** para manejar la autenticación de usuarios.

En mi canal está el video del paso a paso.

https://youtu.be/FoPqycHS2wo

## 🚀 Requisitos

- .NET 7 o superior
- Una cuenta en [Auth0](https://auth0.com)

## ⚙️ Configuración en `program.cs`

Agrega la autenticación de Auth0:

```csharp
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"];
    options.ClientId = builder.Configuration["Auth0:ClientId"];
});

Configura las rutas de Login y Logout:

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

Activa autenticación y autorización en la aplicación:

"Auth0": {
  "Domain": "TU_DOMINIO.auth0.com",
  "ClientId": "TU_CLIENT_ID"
}



