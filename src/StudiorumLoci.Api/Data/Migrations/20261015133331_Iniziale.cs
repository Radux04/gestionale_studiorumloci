using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudiorumLoci.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Iniziale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dipendenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Cognome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Telefono = table.Column<string>(type: "TEXT", nullable: true),
                    CodiceFiscale = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Ruolo = table.Column<string>(type: "TEXT", nullable: false),
                    Contratto = table.Column<string>(type: "TEXT", nullable: false),
                    DataAssunzione = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DataCessazione = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CompensoOrario = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Materie = table.Column<string>(type: "TEXT", nullable: true),
                    Attivo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatoIl = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dipendenti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genitori",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Cognome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Telefono = table.Column<string>(type: "TEXT", nullable: true),
                    CodiceFiscale = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Indirizzo = table.Column<string>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    CreatoIl = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genitori", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Studenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Cognome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DataNascita = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Scuola = table.Column<string>(type: "TEXT", nullable: true),
                    Classe = table.Column<string>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    Attivo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatoIl = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Studenti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Turni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DipendenteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Inizio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Fine = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    CreatoIl = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turni_Dipendenti_DipendenteId",
                        column: x => x.DipendenteId,
                        principalTable: "Dipendenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lead",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Telefono = table.Column<string>(type: "TEXT", nullable: true),
                    Messaggio = table.Column<string>(type: "TEXT", nullable: true),
                    Fonte = table.Column<string>(type: "TEXT", nullable: false),
                    Stato = table.Column<string>(type: "TEXT", nullable: false),
                    GenitoreId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatoIl = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lead", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lead_Genitori_GenitoreId",
                        column: x => x.GenitoreId,
                        principalTable: "Genitori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Pagamenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudenteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Causale = table.Column<string>(type: "TEXT", nullable: false),
                    Importo = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Scadenza = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Stato = table.Column<string>(type: "TEXT", nullable: false),
                    DataPagamento = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Metodo = table.Column<string>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    CreatoIl = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagamenti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagamenti_Studenti_StudenteId",
                        column: x => x.StudenteId,
                        principalTable: "Studenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudenteGenitore",
                columns: table => new
                {
                    GenitoriId = table.Column<int>(type: "INTEGER", nullable: false),
                    StudentiId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudenteGenitore", x => new { x.GenitoriId, x.StudentiId });
                    table.ForeignKey(
                        name: "FK_StudenteGenitore_Genitori_GenitoriId",
                        column: x => x.GenitoriId,
                        principalTable: "Genitori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudenteGenitore_Studenti_StudentiId",
                        column: x => x.StudentiId,
                        principalTable: "Studenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Interazioni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Descrizione = table.Column<string>(type: "TEXT", nullable: false),
                    LeadId = table.Column<int>(type: "INTEGER", nullable: true),
                    GenitoreId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatoIl = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interazioni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interazioni_Genitori_GenitoreId",
                        column: x => x.GenitoreId,
                        principalTable: "Genitori",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Interazioni_Lead_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Lead",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Interazioni_GenitoreId",
                table: "Interazioni",
                column: "GenitoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Interazioni_LeadId",
                table: "Interazioni",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Lead_GenitoreId",
                table: "Lead",
                column: "GenitoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamenti_StudenteId",
                table: "Pagamenti",
                column: "StudenteId");

            migrationBuilder.CreateIndex(
                name: "IX_StudenteGenitore_StudentiId",
                table: "StudenteGenitore",
                column: "StudentiId");

            migrationBuilder.CreateIndex(
                name: "IX_Turni_DipendenteId",
                table: "Turni",
                column: "DipendenteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Interazioni");

            migrationBuilder.DropTable(
                name: "Pagamenti");

            migrationBuilder.DropTable(
                name: "StudenteGenitore");

            migrationBuilder.DropTable(
                name: "Turni");

            migrationBuilder.DropTable(
                name: "Lead");

            migrationBuilder.DropTable(
                name: "Studenti");

            migrationBuilder.DropTable(
                name: "Dipendenti");

            migrationBuilder.DropTable(
                name: "Genitori");
        }
    }
}
