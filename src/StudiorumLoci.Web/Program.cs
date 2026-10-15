using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StudiorumLoci.Web;
using StudiorumLoci.Web.Services;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("it-IT");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Indirizzo dell'API: wwwroot/appsettings.json (sovrascrivibile con appsettings.{Ambiente}.json).
var apiUrl = builder.Configuration["ApiUrl"] ?? throw new InvalidOperationException("ApiUrl non configurato.");
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiUrl) });
builder.Services.AddScoped<ApiClient>();

await builder.Build().RunAsync();
