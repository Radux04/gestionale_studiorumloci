# gestionale_StudiorumLoci

Gestionale per il doposcuola **Studiorum Loci**: API REST in C# (ASP.NET Core, .NET 10) con CRM, gestione dipendenti e gestione pagamenti.
Il sito pubblico [studiorum-loci.vercel.app](https://studiorum-loci.vercel.app) comunica con questa API.

## Struttura

```
src/StudiorumLoci.Api/
├── Domain/          Entità del dominio
│   ├── Crm/         Studente, Genitore, Lead, Interazione
│   ├── Dipendenti/  Dipendente, Turno
│   └── Pagamenti/   Pagamento
├── Data/            AppDbContext (EF Core) e migrazioni
├── Contracts/       DTO di input/output e mapping
└── Controllers/     Endpoint REST
```

## Moduli ed endpoint

| Modulo | Endpoint | Note |
|---|---|---|
| CRM – Studenti | `api/crm/studenti` | CRUD, la DELETE disattiva |
| CRM – Genitori | `api/crm/genitori` | CRUD + `/{id}/interazioni` |
| CRM – Lead | `api/crm/lead` | elenco per stato, `/{id}/stato`, `/{id}/converti`, `/{id}/interazioni` |
| Sito pubblico | `POST api/pubblico/contatti` | form contatti → lead (rate limit 5/min per IP) |
| Dipendenti | `api/dipendenti` | CRUD, `/{id}/turni`, `/{id}/compenso?da=&a=` |
| Pagamenti | `api/pagamenti` | CRUD, `/{id}/incasso`, `/{id}/annulla`, `/riepilogo?anno=&mese=` |

## Avvio in locale

Requisiti: [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet tool restore
dotnet run --project src/StudiorumLoci.Api --launch-profile http
```

- Documentazione interattiva: http://localhost:5078/scalar
- In sviluppo il database SQLite (`studiorumloci.db`) viene creato/aggiornato all'avvio.
- Esempi di chiamate pronti: [src/StudiorumLoci.Api/StudiorumLoci.Api.http](src/StudiorumLoci.Api/StudiorumLoci.Api.http)

Nuova migrazione dopo aver modificato le entità:

```bash
dotnet ef migrations add NomeMigrazione -p src/StudiorumLoci.Api -s src/StudiorumLoci.Api -o Data/Migrations
```

## Configurazione

`appsettings.json`:
- `ConnectionStrings:Gestionale` – stringa di connessione al database
- `Cors:Origini` – domini autorizzati a chiamare l'API dal browser (il sito Vercel)

## Da fare

- [ ] Autenticazione dello staff (es. ASP.NET Core Identity) e `[Authorize]` su tutti i controller tranne `PubblicoController`
- [ ] Database di produzione (es. PostgreSQL) e hosting dell'API (Vercel non esegue .NET: Azure App Service, Render, Railway, Fly.io…)
- [ ] Iscrizioni a corsi/fasce orarie e presenze degli studenti
- [ ] Generazione automatica delle rette mensili e ricevute
- [ ] Interfaccia per lo staff (Blazor o pagina riservata sul sito)
- [ ] Test automatici
