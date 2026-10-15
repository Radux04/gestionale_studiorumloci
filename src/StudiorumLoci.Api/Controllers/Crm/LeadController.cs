using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudiorumLoci.Api.Contracts;
using StudiorumLoci.Api.Data;
using StudiorumLoci.Api.Domain.Crm;

namespace StudiorumLoci.Api.Controllers.Crm;

[ApiController]
[Route("api/crm/lead")]
public class LeadController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<LeadDto>> Elenco(StatoLead? stato)
    {
        var query = db.Lead.AsNoTracking();
        if (stato is not null) query = query.Where(l => l.Stato == stato);

        return (await query.OrderByDescending(l => l.CreatoIl).ToListAsync()).Select(l => l.ToDto());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LeadDto>> Dettaglio(int id)
    {
        var lead = await db.Lead.FindAsync(id);
        return lead is null ? NotFound() : lead.ToDto();
    }

    [HttpPost]
    public async Task<ActionResult<LeadDto>> Crea(LeadInput input)
    {
        var lead = new Lead
        {
            Nome = input.Nome,
            Email = input.Email,
            Telefono = input.Telefono,
            Messaggio = input.Messaggio,
            Fonte = input.Fonte,
        };
        db.Lead.Add(lead);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Dettaglio), new { id = lead.Id }, lead.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LeadDto>> Modifica(int id, LeadInput input)
    {
        var lead = await db.Lead.FindAsync(id);
        if (lead is null) return NotFound();

        lead.Nome = input.Nome;
        lead.Email = input.Email;
        lead.Telefono = input.Telefono;
        lead.Messaggio = input.Messaggio;
        lead.Fonte = input.Fonte;
        await db.SaveChangesAsync();
        return lead.ToDto();
    }

    /// <summary>Elimina il lead e il suo storico contatti (il genitore eventualmente creato resta).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Elimina(int id)
    {
        var lead = await db.Lead.FindAsync(id);
        if (lead is null) return NotFound();

        // Le interazioni già passate a un genitore restano a lui.
        await db.Interazioni.Where(i => i.LeadId == id && i.GenitoreId != null)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.LeadId, (int?)null));

        db.Lead.Remove(lead);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:int}/stato")]
    public async Task<ActionResult<LeadDto>> CambiaStato(int id, CambiaStatoLeadInput input)
    {
        var lead = await db.Lead.FindAsync(id);
        if (lead is null) return NotFound();

        lead.Stato = input.Stato;
        await db.SaveChangesAsync();
        return lead.ToDto();
    }

    /// <summary>Converte il lead in un genitore (anagrafica CRM) e lo marca come convertito.</summary>
    [HttpPost("{id:int}/converti")]
    public async Task<ActionResult<GenitoreDto>> Converti(int id, GenitoreInput input)
    {
        var lead = await db.Lead.FindAsync(id);
        if (lead is null) return NotFound();
        if (lead.GenitoreId is not null) return Conflict("Lead già convertito.");

        var genitore = new Genitore
        {
            Nome = input.Nome,
            Cognome = input.Cognome,
            Email = input.Email ?? lead.Email,
            Telefono = input.Telefono ?? lead.Telefono,
            CodiceFiscale = input.CodiceFiscale,
            Indirizzo = input.Indirizzo,
            Note = input.Note,
        };
        lead.Genitore = genitore;
        lead.Stato = StatoLead.Convertito;

        // Lo storico dei contatti segue il nuovo genitore.
        await db.Interazioni.Where(i => i.LeadId == id).ForEachAsync(i => i.Genitore = genitore);

        await db.SaveChangesAsync();
        return genitore.ToDto();
    }

    [HttpGet("{id:int}/interazioni")]
    public async Task<IEnumerable<InterazioneDto>> Interazioni(int id) =>
        (await db.Interazioni.AsNoTracking().Where(i => i.LeadId == id).OrderByDescending(i => i.Data).ToListAsync())
            .Select(i => i.ToDto());

    [HttpPost("{id:int}/interazioni")]
    public async Task<ActionResult<InterazioneDto>> AggiungiInterazione(int id, InterazioneInput input)
    {
        var lead = await db.Lead.FindAsync(id);
        if (lead is null) return NotFound();

        var interazione = new Interazione
        {
            LeadId = id,
            Tipo = input.Tipo,
            Descrizione = input.Descrizione,
            Data = input.Data?.ToUniversalTime() ?? DateTime.UtcNow,
        };
        db.Interazioni.Add(interazione);
        if (lead.Stato == StatoLead.Nuovo) lead.Stato = StatoLead.Contattato;

        await db.SaveChangesAsync();
        return interazione.ToDto();
    }
}
