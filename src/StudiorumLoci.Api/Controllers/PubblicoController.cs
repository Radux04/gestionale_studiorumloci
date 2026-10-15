using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StudiorumLoci.Api.Contracts;
using StudiorumLoci.Api.Data;
using StudiorumLoci.Api.Domain.Crm;

namespace StudiorumLoci.Api.Controllers;

/// <summary>
/// Endpoint esposti al sito pubblico (studiorum-loci.vercel.app).
/// Sono gli unici endpoint accessibili senza login.
/// </summary>
[ApiController]
[Route("api/pubblico")]
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.Pubblico)]
public class PubblicoController(AppDbContext db) : ControllerBase
{
    /// <summary>Riceve il form "Contattaci" del sito e lo registra come lead nel CRM.</summary>
    [HttpPost("contatti")]
    public async Task<IActionResult> RichiestaContatto(RichiestaContattoInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Email) && string.IsNullOrWhiteSpace(input.Telefono))
            return ValidationProblem("Indicare almeno email o telefono.");

        db.Lead.Add(new Lead
        {
            Nome = input.Nome,
            Email = input.Email,
            Telefono = input.Telefono,
            Messaggio = input.Messaggio,
            Fonte = FonteLead.Sito,
        });
        await db.SaveChangesAsync();
        return Accepted();
    }
}
