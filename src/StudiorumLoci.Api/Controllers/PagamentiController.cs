using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudiorumLoci.Api.Contracts;
using StudiorumLoci.Api.Data;
using StudiorumLoci.Api.Domain.Pagamenti;

namespace StudiorumLoci.Api.Controllers;

[ApiController]
[Route("api/pagamenti")]
public class PagamentiController(AppDbContext db, TimeProvider clock) : ControllerBase
{
    private DateOnly Oggi => DateOnly.FromDateTime(clock.GetLocalNow().DateTime);

    [HttpGet]
    public async Task<IEnumerable<PagamentoDto>> Elenco(int? studenteId, StatoPagamento? stato, bool soloScaduti = false)
    {
        var oggi = Oggi;
        var query = db.Pagamenti.Include(p => p.Studente).AsNoTracking();
        if (studenteId is not null) query = query.Where(p => p.StudenteId == studenteId);
        if (stato is not null) query = query.Where(p => p.Stato == stato);
        if (soloScaduti) query = query.Where(p => p.Stato == StatoPagamento.DaPagare && p.Scadenza < oggi);

        return (await query.OrderBy(p => p.Scadenza).ToListAsync()).Select(p => p.ToDto(oggi));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PagamentoDto>> Dettaglio(int id)
    {
        var pagamento = await Trova(id);
        return pagamento is null ? NotFound() : pagamento.ToDto(Oggi);
    }

    [HttpPost]
    public async Task<ActionResult<PagamentoDto>> Crea(PagamentoInput input)
    {
        if (!await db.Studenti.AnyAsync(s => s.Id == input.StudenteId))
            return ValidationProblem($"Studente {input.StudenteId} inesistente.");

        var pagamento = new Pagamento
        {
            StudenteId = input.StudenteId,
            Causale = input.Causale,
            Importo = input.Importo,
            Scadenza = input.Scadenza,
            Note = input.Note,
        };
        db.Pagamenti.Add(pagamento);
        await db.SaveChangesAsync();
        await db.Entry(pagamento).Reference(p => p.Studente).LoadAsync();
        return CreatedAtAction(nameof(Dettaglio), new { id = pagamento.Id }, pagamento.ToDto(Oggi));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PagamentoDto>> Modifica(int id, PagamentoInput input)
    {
        var pagamento = await Trova(id);
        if (pagamento is null) return NotFound();
        if (!await db.Studenti.AnyAsync(s => s.Id == input.StudenteId))
            return ValidationProblem($"Studente {input.StudenteId} inesistente.");

        pagamento.StudenteId = input.StudenteId;
        pagamento.Causale = input.Causale;
        pagamento.Importo = input.Importo;
        pagamento.Scadenza = input.Scadenza;
        pagamento.Note = input.Note;
        await db.SaveChangesAsync();
        await db.Entry(pagamento).Reference(p => p.Studente).LoadAsync();
        return pagamento.ToDto(Oggi);
    }

    /// <summary>Registra l'incasso di una quota.</summary>
    [HttpPost("{id:int}/incasso")]
    public async Task<ActionResult<PagamentoDto>> RegistraIncasso(int id, RegistraIncassoInput input)
    {
        var pagamento = await Trova(id);
        if (pagamento is null) return NotFound();
        if (pagamento.Stato != StatoPagamento.DaPagare) return Conflict($"Pagamento in stato {pagamento.Stato}.");

        pagamento.Stato = StatoPagamento.Pagato;
        pagamento.Metodo = input.Metodo;
        pagamento.DataPagamento = input.DataPagamento ?? Oggi;
        await db.SaveChangesAsync();
        return pagamento.ToDto(Oggi);
    }

    [HttpPost("{id:int}/annulla")]
    public async Task<ActionResult<PagamentoDto>> Annulla(int id)
    {
        var pagamento = await Trova(id);
        if (pagamento is null) return NotFound();
        if (pagamento.Stato == StatoPagamento.Pagato) return Conflict("Un pagamento già incassato non può essere annullato.");

        pagamento.Stato = StatoPagamento.Annullato;
        await db.SaveChangesAsync();
        return pagamento.ToDto(Oggi);
    }

    /// <summary>Riporta a "da pagare" un pagamento incassato o annullato per errore.</summary>
    [HttpPost("{id:int}/ripristina")]
    public async Task<ActionResult<PagamentoDto>> Ripristina(int id)
    {
        var pagamento = await Trova(id);
        if (pagamento is null) return NotFound();

        pagamento.Stato = StatoPagamento.DaPagare;
        pagamento.DataPagamento = null;
        pagamento.Metodo = null;
        await db.SaveChangesAsync();
        return pagamento.ToDto(Oggi);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Elimina(int id)
    {
        var pagamento = await db.Pagamenti.FindAsync(id);
        if (pagamento is null) return NotFound();

        db.Pagamenti.Remove(pagamento);
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Riepilogo delle quote con scadenza nel mese indicato.</summary>
    [HttpGet("riepilogo")]
    public async Task<ActionResult<RiepilogoPagamentiDto>> Riepilogo(int anno, int mese)
    {
        if (mese is < 1 or > 12 || anno is < 2000 or > 2100) return ValidationProblem("Anno o mese non validi.");

        var oggi = Oggi;
        var inizio = new DateOnly(anno, mese, 1);
        var fine = inizio.AddMonths(1);

        var pagamenti = await db.Pagamenti.AsNoTracking()
            .Where(p => p.Scadenza >= inizio && p.Scadenza < fine && p.Stato != StatoPagamento.Annullato)
            .ToListAsync();

        var atteso = pagamenti.Sum(p => p.Importo);
        var incassato = pagamenti.Where(p => p.Stato == StatoPagamento.Pagato).Sum(p => p.Importo);
        return new RiepilogoPagamentiDto(anno, mese, atteso, incassato, atteso - incassato, pagamenti.Count(p => p.Scaduto(oggi)));
    }

    private Task<Pagamento?> Trova(int id) =>
        db.Pagamenti.Include(p => p.Studente).FirstOrDefaultAsync(p => p.Id == id);
}
