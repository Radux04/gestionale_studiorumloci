using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudiorumLoci.Api.Contracts;
using StudiorumLoci.Api.Data;
using StudiorumLoci.Api.Domain.Crm;

namespace StudiorumLoci.Api.Controllers.Crm;

[ApiController]
[Route("api/crm/genitori")]
public class GenitoriController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<GenitoreDto>> Elenco(string? cerca)
    {
        var query = db.Genitori.Include(g => g.Studenti).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(cerca))
            query = query.Where(g => g.Nome.Contains(cerca) || g.Cognome.Contains(cerca) || (g.Email != null && g.Email.Contains(cerca)));

        var genitori = await query.OrderBy(g => g.Cognome).ThenBy(g => g.Nome).ToListAsync();
        return genitori.Select(g => g.ToDto());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GenitoreDto>> Dettaglio(int id)
    {
        var genitore = await db.Genitori.Include(g => g.Studenti).AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
        return genitore is null ? NotFound() : genitore.ToDto();
    }

    [HttpPost]
    public async Task<ActionResult<GenitoreDto>> Crea(GenitoreInput input)
    {
        var genitore = new Genitore { Nome = input.Nome, Cognome = input.Cognome };
        Applica(genitore, input);
        db.Genitori.Add(genitore);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Dettaglio), new { id = genitore.Id }, genitore.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<GenitoreDto>> Modifica(int id, GenitoreInput input)
    {
        var genitore = await db.Genitori.Include(g => g.Studenti).FirstOrDefaultAsync(g => g.Id == id);
        if (genitore is null) return NotFound();

        Applica(genitore, input);
        await db.SaveChangesAsync();
        return genitore.ToDto();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Elimina(int id)
    {
        var genitore = await db.Genitori.FindAsync(id);
        if (genitore is null) return NotFound();

        db.Genitori.Remove(genitore);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:int}/interazioni")]
    public async Task<IEnumerable<InterazioneDto>> Interazioni(int id) =>
        (await db.Interazioni.AsNoTracking().Where(i => i.GenitoreId == id).OrderByDescending(i => i.Data).ToListAsync())
            .Select(i => i.ToDto());

    [HttpPost("{id:int}/interazioni")]
    public async Task<ActionResult<InterazioneDto>> AggiungiInterazione(int id, InterazioneInput input)
    {
        if (!await db.Genitori.AnyAsync(g => g.Id == id)) return NotFound();

        var interazione = new Interazione
        {
            GenitoreId = id,
            Tipo = input.Tipo,
            Descrizione = input.Descrizione,
            Data = input.Data?.ToUniversalTime() ?? DateTime.UtcNow,
        };
        db.Interazioni.Add(interazione);
        await db.SaveChangesAsync();
        return interazione.ToDto();
    }

    private static void Applica(Genitore g, GenitoreInput input)
    {
        g.Nome = input.Nome;
        g.Cognome = input.Cognome;
        g.Email = input.Email;
        g.Telefono = input.Telefono;
        g.CodiceFiscale = input.CodiceFiscale?.ToUpperInvariant();
        g.Indirizzo = input.Indirizzo;
        g.Note = input.Note;
    }
}
