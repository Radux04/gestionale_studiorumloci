using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudiorumLoci.Api.Data;
using StudiorumLoci.Api.Domain.Accessi;

namespace StudiorumLoci.Api.Auth;

public static class ConfigurazioneAccessi
{
    public static IServiceCollection AddAccessi(this IServiceCollection services, IConfiguration config)
    {
        var jwt = config.GetSection("Jwt").Get<ImpostazioniJwt>() ?? new ImpostazioniJwt();
        services.AddSingleton(jwt);
        services.AddScoped<ServizioAccessi>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
        {
            o.MapInboundClaims = false;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwt.Emittente,
                ValidAudience = jwt.Emittente,
                // Risolta al primo uso: senza Jwt:Chiave l'API parte comunque (es. per le migrazioni)
                // ma login e richieste autenticate falliscono con un errore esplicito.
                IssuerSigningKeyResolver = (_, _, _, _) => [jwt.ChiaveFirma()],
                NameClaimType = NomiClaim.Nome,
                RoleClaimType = NomiClaim.Ruolo,
                ClockSkew = TimeSpan.FromMinutes(1),
            };
            o.Events = new JwtBearerEvents { OnTokenValidated = VerificaUtente };
        });

        // Tutto richiede il login, salvo gli endpoint marcati [AllowAnonymous].
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return services;
    }

    /// <summary>
    /// A ogni richiesta controlla che l'account sia ancora attivo e che il token non sia stato
    /// invalidato (cambio password/ruolo, blocco). Finché la password temporanea non è stata
    /// cambiata sono consentiti solo gli endpoint /api/auth.
    /// </summary>
    private static async Task VerificaUtente(TokenValidatedContext ctx)
    {
        var principal = ctx.Principal!;
        if (!int.TryParse(principal.FindFirst(NomiClaim.Id)?.Value, out var id)) { ctx.Fail("Token non valido."); return; }

        var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var utente = await db.Utenti.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new { u.Attivo, u.VersioneToken, u.DeveCambiarePassword })
            .FirstOrDefaultAsync();

        if (utente is null || !utente.Attivo || principal.FindFirst(NomiClaim.Versione)?.Value != utente.VersioneToken.ToString())
        {
            ctx.Fail("Sessione non più valida.");
            return;
        }
        if (utente.DeveCambiarePassword && !ctx.HttpContext.Request.Path.StartsWithSegments("/api/auth"))
            ctx.Fail("Cambio password richiesto.");
    }

    /// <summary>
    /// Crea il primo superadmin se configurato (Superadmin:Email e Superadmin:Password) e non esiste
    /// già un account con quell'email. Non modifica mai un account esistente.
    /// </summary>
    public static async Task CreaSuperadminSeConfiguratoAsync(this WebApplication app)
    {
        if (app.Services.GetRequiredService<ImpostazioniJwt>().Chiave.Length < 32)
            app.Logger.LogError("Jwt:Chiave mancante o troppo corta (min. 32 caratteri): il login non funzionerà.");

        var email = app.Configuration["Superadmin:Email"]?.Trim().ToLowerInvariant();
        var password = app.Configuration["Superadmin:Password"];
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (await db.Utenti.AnyAsync(u => u.Email == email)) return;

        if (password.Length < Ruoli.LunghezzaMinimaPassword)
            throw new InvalidOperationException($"Superadmin:Password deve avere almeno {Ruoli.LunghezzaMinimaPassword} caratteri.");

        var superadmin = new Utente { Email = email, Ruolo = RuoloAccesso.SuperAdmin, Nome = "Superadmin", DeveCambiarePassword = false };
        scope.ServiceProvider.GetRequiredService<ServizioAccessi>().ImpostaPassword(superadmin, password);
        db.Utenti.Add(superadmin);
        await db.SaveChangesAsync();
        app.Logger.LogWarning("Creato il superadmin {Email}. Ora puoi rimuovere Superadmin:Password dalla configurazione.", email);
    }
}
