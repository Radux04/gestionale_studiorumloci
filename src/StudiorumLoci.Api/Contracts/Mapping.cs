using StudiorumLoci.Api.Domain.Accessi;
using StudiorumLoci.Api.Domain.Crm;
using StudiorumLoci.Api.Domain.Dipendenti;
using StudiorumLoci.Api.Domain.Pagamenti;
using StudiorumLoci.Api.Domain.Sedi;

namespace StudiorumLoci.Api.Contracts;

/// <summary>Conversione entità → DTO condivisi (StudiorumLoci.Shared).</summary>
public static class Mapping
{
    /// <remarks>Per il nome della sede richiede che <see cref="Studente.Sede"/> sia caricata.</remarks>
    public static StudenteDto ToDto(this Studente s) => new(
        s.Id, s.Nome, s.Cognome, s.DataNascita, s.Scuola, s.Classe, s.Note, s.Attivo,
        s.Genitori.Select(g => g.Id).ToArray(), s.SedeId, s.Sede?.Nome);

    public static SedeDto ToDto(this Sede s, int numeroStudenti, int numeroDipendenti) => new(
        s.Id, s.Nome, s.Indirizzo, s.Cap, s.Comune, s.Provincia, s.Regione,
        s.Telefono, s.Email, s.Note, s.Attiva, numeroStudenti, numeroDipendenti);

    public static GenitoreDto ToDto(this Genitore g) => new(
        g.Id, g.Nome, g.Cognome, g.Email, g.Telefono, g.CodiceFiscale, g.Indirizzo, g.Note,
        g.Studenti.Select(s => s.Id).ToArray());

    public static LeadDto ToDto(this Lead l) => new(
        l.Id, l.Nome, l.Email, l.Telefono, l.Messaggio, l.Fonte, l.Stato, l.GenitoreId, l.CreatoIl);

    public static InterazioneDto ToDto(this Interazione i) => new(
        i.Id, i.Data, i.Tipo, i.Descrizione, i.LeadId, i.GenitoreId);

    public static DipendenteDto ToDto(this Dipendente d) => new(
        d.Id, d.Nome, d.Cognome, d.Email, d.Telefono, d.CodiceFiscale, d.Ruolo, d.Contratto,
        d.DataAssunzione, d.DataCessazione, d.CompensoOrario, d.Materie, d.Attivo,
        d.Sedi.Select(s => s.Id).ToArray(),
        d.Utente is { } u ? new AccessoDto(u.Id, u.Email, u.Ruolo, u.Attivo, u.DeveCambiarePassword, u.UltimoAccesso) : null);

    /// <remarks>Per il nome richiede che <see cref="Utente.Dipendente"/> sia caricato.</remarks>
    public static UtenteDto ToDto(this Utente u) => new(
        u.Id, u.Email, u.NomeVisualizzato, u.Ruolo, u.Attivo, u.DeveCambiarePassword, u.UltimoAccesso, u.DipendenteId);

    public static UtenteCorrenteDto ToCorrenteDto(this Utente u) => new(
        u.Id, u.Email, u.NomeVisualizzato, u.Ruolo, u.DipendenteId, u.DeveCambiarePassword);

    public static TurnoDto ToDto(this Turno t) => new(t.Id, t.DipendenteId, t.Inizio, t.Fine, t.Ore, t.Note);

    /// <remarks>Richiede che <see cref="Pagamento.Studente"/> sia caricato.</remarks>
    public static PagamentoDto ToDto(this Pagamento p, DateOnly oggi) => new(
        p.Id, p.StudenteId, $"{p.Studente.Cognome} {p.Studente.Nome}", p.Causale, p.Importo, p.Scadenza,
        p.Stato, p.Scaduto(oggi), p.DataPagamento, p.Metodo, p.Note,
        p.RicevutaNome is null ? null : new RicevutaDto(p.RicevutaNome, p.RicevutaTipo!, p.RicevutaDimensione ?? 0));
}
