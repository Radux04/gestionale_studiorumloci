using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudiorumLoci.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Calendario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lezioni",
                schema: "gestionale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titolo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Inizio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Fine = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SedeId = table.Column<int>(type: "integer", nullable: false),
                    DipendenteId = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    SerieId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lezioni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lezioni_Dipendenti_DipendenteId",
                        column: x => x.DipendenteId,
                        principalSchema: "gestionale",
                        principalTable: "Dipendenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Lezioni_Sedi_SedeId",
                        column: x => x.SedeId,
                        principalSchema: "gestionale",
                        principalTable: "Sedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LezioniStudenti",
                schema: "gestionale",
                columns: table => new
                {
                    LezioneId = table.Column<int>(type: "integer", nullable: false),
                    StudenteId = table.Column<int>(type: "integer", nullable: false),
                    Presente = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LezioniStudenti", x => new { x.LezioneId, x.StudenteId });
                    table.ForeignKey(
                        name: "FK_LezioniStudenti_Lezioni_LezioneId",
                        column: x => x.LezioneId,
                        principalSchema: "gestionale",
                        principalTable: "Lezioni",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LezioniStudenti_Studenti_StudenteId",
                        column: x => x.StudenteId,
                        principalSchema: "gestionale",
                        principalTable: "Studenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lezioni_DipendenteId",
                schema: "gestionale",
                table: "Lezioni",
                column: "DipendenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Lezioni_Inizio",
                schema: "gestionale",
                table: "Lezioni",
                column: "Inizio");

            migrationBuilder.CreateIndex(
                name: "IX_Lezioni_SedeId",
                schema: "gestionale",
                table: "Lezioni",
                column: "SedeId");

            migrationBuilder.CreateIndex(
                name: "IX_LezioniStudenti_StudenteId",
                schema: "gestionale",
                table: "LezioniStudenti",
                column: "StudenteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LezioniStudenti",
                schema: "gestionale");

            migrationBuilder.DropTable(
                name: "Lezioni",
                schema: "gestionale");
        }
    }
}
