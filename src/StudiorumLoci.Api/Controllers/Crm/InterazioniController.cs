using Microsoft.AspNetCore.Mvc;
using StudiorumLoci.Api.Contracts;
using StudiorumLoci.Api.Data;

namespace StudiorumLoci.Api.Controllers.Crm;

/// <summary>Modifica ed eliminazione delle voci dello storico contatti (create da lead o genitori).</summary>
[ApiController]
[Route("api/crm/interazioni")]
public class InterazioniController(AppDbContext db) : ControllerBase
{
    [HttpPut("{id:int}")]
    public async Task<ActionResult<InterazioneDto>> Modifica(int id, InterazioneInput input)
    {
        var interazione = await db.Interazioni.FindAsync(id);
        if (interazione is null) return NotFound();

        interazione.Tipo = input.Tipo;
        interazione.Descrizione = input.Descrizione;
        if (input.Data is { } data) interazione.Data = data.ToUniversalTime();
        await db.SaveChangesAsync();
        return interazione.ToDto();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Elimina(int id)
    {
        var interazione = await db.Interazioni.FindAsync(id);
        if (interazione is null) return NotFound();

        db.Interazioni.Remove(interazione);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
