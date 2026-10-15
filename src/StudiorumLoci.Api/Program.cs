using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StudiorumLoci.Api;
using StudiorumLoci.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// La password del DB non sta in appsettings: arriva da user-secrets (sviluppo)
// o dalla variabile d'ambiente Database__Password (produzione).
var connessioneDb = ConnessioneDb.Costruisci(
    builder.Configuration.GetConnectionString("Gestionale"),
    builder.Configuration["Database:Password"]);
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connessioneDb));

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Il sito pubblico (Vercel) chiama l'API dal browser: va autorizzato via CORS.
const string CorsSito = "Sito";
builder.Services.AddCors(o => o.AddPolicy(CorsSito, p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:Origini").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

// Limita gli invii del form contatti per IP, per scoraggiare lo spam.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy(RateLimitPolicies.Pubblico, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "sconosciuto",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) }));
});

// In produzione l'API sta dietro il proxy dell'hosting (Render): l'IP reale del
// visitatore e lo schema https arrivano negli header X-Forwarded-*.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

// TODO: autenticazione per lo staff (es. ASP.NET Core Identity + cookie/JWT) e
// [Authorize] su tutti i controller tranne PubblicoController.

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // UI di prova: /scalar

    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}
else
{
    app.UseForwardedHeaders();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors(CorsSito);
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

// Usato dall'hosting per verificare che l'API sia attiva.
app.MapGet("/salute", () => Results.Ok("ok"));

app.Run();
