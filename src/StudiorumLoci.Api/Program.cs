using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StudiorumLoci.Api;
using StudiorumLoci.Api.Auth;
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

// Sito pubblico e gestionale (Vercel) chiamano l'API dal browser: vanno autorizzati via CORS.
const string CorsSito = "Sito";
var originiConsentite = new OriginiConsentite(builder.Configuration.GetSection("Cors:Origini").Get<string[]>() ?? []);
builder.Services.AddCors(o => o.AddPolicy(CorsSito, p => p
    .SetIsOriginAllowed(originiConsentite.Consente)
    .AllowAnyHeader()
    .AllowAnyMethod()));

// Limiti per IP: form contatti (anti-spam) e login/cambio password (anti tentativi a ripetizione).
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy(RateLimitPolicies.Pubblico, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "sconosciuto",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) }));
    o.AddPolicy(RateLimitPolicies.Login, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "sconosciuto",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

// In produzione l'API sta dietro il proxy dell'hosting (Render): l'IP reale del
// visitatore e lo schema https arrivano negli header X-Forwarded-*.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

// Login con token (Jwt:Chiave da user-secrets / variabile d'ambiente Jwt__Chiave).
// Ogni endpoint richiede l'accesso, salvo quelli marcati [AllowAnonymous].
builder.Services.AddAccessi(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous(); // UI di prova: /scalar

    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}
else
{
    app.UseForwardedHeaders();
}

// CORS prima della gestione errori: anche le risposte di errore devono arrivare al browser.
app.UseCors(CorsSito);
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Usato dall'hosting per verificare che l'API sia attiva.
app.MapGet("/salute", () => Results.Ok("ok")).AllowAnonymous();

await app.CreaSuperadminSeConfiguratoAsync();

app.Run();
