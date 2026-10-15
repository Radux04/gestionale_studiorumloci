using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudiorumLoci.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Iniziale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gestionale");

            migrationBuilder.CreateTable(
                name: "Dipendenti",
                schema: "gestionale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cognome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    CodiceFiscale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Ruolo = table.Column<string>(type: "text", nullable: false),
                    Contratto = table.Column<string>(type: "text", nullable: false),
                    DataAssunzione = table.Column<DateOnly>(type: "date", nullable: false),
                    DataCessazione = table.Column<DateOnly>(type: "date", nullable: true),
                    CompensoOrario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Materie = table.Column<string>(type: "text", nullable: true),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dipendenti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genitori",
                schema: "gestionale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cognome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    CodiceFiscale = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Indirizzo = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genitori", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Studenti",
                schema: "gestionale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cognome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DataNascita = table.Column<DateOnly>(type: "date", nullable: true),
                    Scuola = table.Column<string>(type: "text", nullable: true),
                    Classe = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Studenti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Turni",
                schema: "gestionale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DipendenteId = table.Column<int>(type: "integer", nullable: false),
                    Inizio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Fine = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turni_Dipendenti_DipendenteId",
                        column: x => x.DipendenteId,
                        principalSchema: "gestionale",
                        principalTable: "Dipendenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lead",
                schema: "gestionale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    Messaggio = table.Column<string>(type: "text", nullable: true),
                    Fonte = table.Column<string>(type: "text", nullable: false),
                    Stato = table.Column<string>(type: "text", nullable: false),
                    GenitoreId = table.Column<int>(type: "integer", nullable: true),
                    CreatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lead", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lead_Genitori_GenitoreId",
                        column: x => x.GenitoreId,
                        principalSchema: "gestionale",
                        principalTable: "Genitori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Pagamenti",
                schema: "gestionale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudenteId = table.Column<int>(type: "integer", nullable: false),
                    Causale = table.Column<string>(type: "text", nullable: false),
                    Importo = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Scadenza = table.Column<DateOnly>(type: "date", nullable: false),
                    Stato = table.Column<string>(type: "text", nullable: false),
                    DataPagamento = table.Column<DateOnly>(type: "date", nullable: true),
                    Metodo = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagamenti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagamenti_Studenti_StudenteId",
                        column: x => x.StudenteId,
                        principalSchema: "gestionale",
                        principalTable: "Studenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudenteGenitore",
                schema: "gestionale",
                columns: table => new
                {
                    GenitoriId = table.Column<int>(type: "integer", nullable: false),
                    StudentiId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudenteGenitore", x => new { x.GenitoriId, x.StudentiId });
                    table.ForeignKey(
                        name: "FK_StudenteGenitore_Genitori_GenitoriId",
                        column: x => x.GenitoriId,
                        principalSchema: "gestionale",
                        principalTable: "Genitori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudenteGenitore_Studenti_StudentiId",
                        column: x => x.StudentiId,
                        principalSchema: "gestionale",
                        principalTable: "Studenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Interazioni",
                schema: "gestionale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Descrizione = table.Column<string>(type: "text", nullable: false),
                    LeadId = table.Column<int>(type: "integer", nullable: true),
                    GenitoreId = table.Column<int>(type: "integer", nullable: true),
                    CreatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interazioni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interazioni_Genitori_GenitoreId",
                        column: x => x.GenitoreId,
                        principalSchema: "gestionale",
                        principalTable: "Genitori",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Interazioni_Lead_LeadId",
                        column: x => x.LeadId,
                        principalSchema: "gestionale",
                        principalTable: "Lead",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Interazioni_GenitoreId",
                schema: "gestionale",
                table: "Interazioni",
                column: "GenitoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Interazioni_LeadId",
                schema: "gestionale",
                table: "Interazioni",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_GenitoreId",
                schema: "gestionale",
                table: "Lead",
                column: "GenitoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamenti_StudenteId",
                schema: "gestionale",
                table: "Pagamenti",
                column: "StudenteId");

            migrationBuilder.CreateIndex(
                name: "IX_StudenteGenitore_StudentiId",
                schema: "gestionale",
                table: "StudenteGenitore",
                column: "StudentiId");

            migrationBuilder.CreateIndex(
                name: "IX_Turni_DipendenteId",
                schema: "gestionale",
                table: "Turni",
                column: "DipendenteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Interazioni",
                schema: "gestionale");

            migrationBuilder.DropTable(
                name: "Pagamenti",
                schema: "gestionale");

            migrationBuilder.DropTable(
                name: "StudenteGenitore",
                schema: "gestionale");

            migrationBuilder.DropTable(
                name: "Turni",
                schema: "gestionale");

            migrationBuilder.DropTable(
                name: "Lead",
                schema: "gestionale");

            migrationBuilder.DropTable(
                name: "Studenti",
                schema: "gestionale");

            migrationBuilder.DropTable(
                name: "Dipendenti",
                schema: "gestionale");

            migrationBuilder.DropTable(
                name: "Genitori",
                schema: "gestionale");
        }
    }
}
