using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StudiorumLoci.Web;
using StudiorumLoci.Web.Services;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("it-IT");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Sessione e login: [Authorize] sulle pagine, <AuthorizeView> nei componenti.
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<Sessione>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<Sessione>());

// Indirizzo dell'API: wwwroot/appsettings.json (sovrascrivibile con appsettings.{Ambiente}.json).
var apiUrl = builder.Configuration["ApiUrl"] ?? throw new InvalidOperationException("ApiUrl non configurato.");
builder.Services.AddScoped(sp => new HttpClient(new AggiungiToken(
        sp.GetRequiredService<Sessione>(),
        sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>())
    { InnerHandler = new HttpClientHandler() })
{
    BaseAddress = new Uri(apiUrl),
});
builder.Services.AddScoped<ApiClient>();

await builder.Build().RunAsync();
