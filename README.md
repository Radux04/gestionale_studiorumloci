# gestionale_StudiorumLoci

Gestionale per il doposcuola **Studiorum Loci**, interamente in C# (.NET 10): CRM, gestione dipendenti e gestione pagamenti.
Il sito pubblico [studiorum-loci.vercel.app](https://studiorum-loci.vercel.app) comunica con l'API.

## Struttura

| Progetto | Cosa contiene |
|---|---|
| `src/StudiorumLoci.Api` | API REST (ASP.NET Core + EF Core, PostgreSQL su Supabase) |
| `src/StudiorumLoci.Web` | Interfaccia per lo staff (Blazor WebAssembly + Bootstrap) |
| `src/StudiorumLoci.Shared` | DTO ed enum condivisi tra API e interfaccia |

### Interfaccia (`StudiorumLoci.Web`)

| Pagina | Percorso |
|---|---|
| Panoramica: richieste nuove, studenti/dipendenti attivi, rette scadute, incassi del mese (filtrabile per sede) | `/?sedeId=` |
| Richieste (lead): filtro per stato, storico contatti, conversione in genitore | `/crm/lead` |
| Studenti: ricerca, filtro per sede, anagrafica, collegamento ai genitori | `/crm/studenti` |
| Genitori: anagrafica e storico contatti | `/crm/genitori` |
| Calendario: settimana con le lezioni per sede/professore, serie settimanali, presenze | `/calendario` |
| Sedi: elenco per regione, anagrafica, numero di studenti | `/sedi` |
| Dipendenti: elenco filtrabile per ruolo e sede, anagrafica con le sedi di lavoro | `/dipendenti` |
| Scheda dipendente: turni del mese e compenso maturato | `/dipendenti/{id}` |
| Pagamenti: quote, incassi, ricevute, riepilogo mensile, filtro per sede | `/pagamenti` |
| Login, cambio password | `/login`, `/cambia-password` |
| Accessi: account, reimposta password, blocca (amministrazione) | `/utenti` |

L'indirizzo dell'API si imposta in `src/StudiorumLoci.Web/wwwroot/appsettings.json` (`ApiUrl`).

### API (`StudiorumLoci.Api`)

```
src/StudiorumLoci.Api/
├── Domain/          Entità del dominio
│   ├── Crm/         Studente, Genitore, Lead, Interazione
│   ├── Dipendenti/  Dipendente, Turno
│   └── Pagamenti/   Pagamento
├── Data/            AppDbContext (EF Core) e migrazioni
├── Contracts/       Mapping entità → DTO
└── Controllers/     Endpoint REST
```

## Moduli ed endpoint

| Modulo | Endpoint | Note |
|---|---|---|
| Calendario | `api/lezioni` | elenco per periodo (`da`, `a`, `sedeId`, `dipendenteId`), CRUD con serie settimanali (`ripetiFinoAl`, `?successive=true`), `/{id}/presenze` |
| Sedi | `api/sedi` | CRUD, filtri `regione` e `soloAttive`; la DELETE è rifiutata se la sede ha studenti |
| CRM – Studenti | `api/crm/studenti` | CRUD (sede obbligatoria), filtro `sedeId` (0 = senza sede), `/{id}/disattiva`; la DELETE è rifiutata se lo studente ha pagamenti |
| CRM – Genitori | `api/crm/genitori` | CRUD (la DELETE elimina anche lo storico contatti) + `/{id}/interazioni` |
| CRM – Lead | `api/crm/lead` | CRUD, `/{id}/stato`, `/{id}/converti`, `/{id}/interazioni` |
| CRM – Storico contatti | `api/crm/interazioni/{id}` | modifica ed eliminazione |
| Accesso | `api/auth` | `POST /login`, `GET /io`, `POST /cambia-password` |
| Account | `api/utenti` | CRUD, `/{id}/reimposta-password` (superadmin e admin) |
| Sito pubblico | `POST api/pubblico/contatti` | form contatti → lead (rate limit 5/min per IP) |
| Dipendenti | `api/dipendenti` | CRUD con `sediIds` (più sedi per dipendente), filtro `sedeId` (0 = senza sede), (la DELETE elimina anche i turni), `/{id}/cessa`, `/{id}/turni` (CRUD), `/{id}/compenso?da=&a=` |
| Pagamenti | `api/pagamenti` | CRUD, filtro `sedeId` (sede dello studente) anche su `/riepilogo`, `/{id}/incasso`, `/{id}/annulla`, `/{id}/ripristina`, `/{id}/ricevuta` (POST multipart / GET / DELETE: foto o PDF, max 10 MB), `/riepilogo?anno=&mese=` |

## Avvio in locale

Requisiti: [.NET 10 SDK](https://dotnet.microsoft.com/download).

In due terminali separati:

```bash
dotnet tool restore
dotnet run --project src/StudiorumLoci.Api --launch-profile http   # API → http://localhost:5078
dotnet run --project src/StudiorumLoci.Web --launch-profile http   # Gestionale → http://localhost:5150
```

- Documentazione interattiva dell'API: http://localhost:5078/scalar
- Esempi di chiamate pronti: [src/StudiorumLoci.Api/StudiorumLoci.Api.http](src/StudiorumLoci.Api/StudiorumLoci.Api.http)

Nuova migrazione dopo aver modificato le entità:

```bash
dotnet ef migrations add NomeMigrazione -p src/StudiorumLoci.Api -s src/StudiorumLoci.Api -o Data/Migrations
```

## Configurazione

Database: PostgreSQL su **Supabase**, tabelle nello schema `gestionale` (non esposto dalla Data API di Supabase).

`appsettings.json`:
- `ConnectionStrings:Gestionale` – stringa di connessione **senza password** (formato `Host=...;Username=...` oppure URI `postgresql://...`)
- `Cors:Origini` – domini autorizzati a chiamare l'API dal browser (il sito Vercel)

La password **non va mai committata**:

```bash
# sviluppo
dotnet user-secrets set "Database:Password" "<password>" --project src/StudiorumLoci.Api
# produzione: variabile d'ambiente
Database__Password=<password>
```

In sviluppo le migrazioni vengono applicate all'avvio; in alternativa:

```bash
dotnet ef database update -p src/StudiorumLoci.Api -s src/StudiorumLoci.Api
```

## Accesso e ruoli

Login con email e password; l'API emette un token (10 ore) che il gestionale invia a ogni richiesta.
Cambiando password, ruolo o bloccando un account, i token già emessi smettono subito di valere.

| | Superadmin | Admin | Manager | Professore |
|---|---|---|---|---|
| CRM, sedi, pagamenti, turni | ✓ | ✓ | ✓ | — |
| Vedere/modificare dipendenti | ✓ | ✓ | ✓ | solo la propria scheda (lettura) |
| Creare, cessare, eliminare dipendenti e gestirne l'accesso | ✓ | ✓ | — | — |
| Gestire account admin/superadmin | ✓ | — | — | — |
| Studenti | ✓ | ✓ | ✓ | solo delle proprie sedi (lettura) |

- Creando un dipendente si sceglie se è **manager** o **professore**; il sistema genera una password temporanea,
  mostrata una sola volta, da cambiare al primo accesso.
- Gli account admin (il cliente) li crea il superadmin dalla pagina **Accessi**.

Configurazione (mai nel codice):

```bash
# chiave di firma dei token, almeno 32 caratteri (su Render è generata in automatico)
dotnet user-secrets set "Jwt:Chiave" "<stringa casuale lunga>" --project src/StudiorumLoci.Api
# primo superadmin: creato all'avvio se non esiste (poi si può rimuovere la password)
dotnet user-secrets set "Superadmin:Email" "<email>" --project src/StudiorumLoci.Api
dotnet user-secrets set "Superadmin:Password" "<password di almeno 10 caratteri>" --project src/StudiorumLoci.Api
```

In produzione le stesse chiavi diventano `Jwt__Chiave`, `Superadmin__Email`, `Superadmin__Password`.

## Da fare

- [ ] Recupero password via email (oggi la reimposta l'amministratore)
- [ ] Iscrizioni a corsi/fasce orarie e presenze degli studenti
- [ ] Generazione automatica delle rette mensili e ricevute
- [ ] Test automatici
