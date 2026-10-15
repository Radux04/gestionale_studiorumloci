using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudiorumLoci.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Accessi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Utenti",
                schema: "gestionale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Ruolo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Attivo = table.Column<bool>(type: "boolean", nullable: false),
                    DeveCambiarePassword = table.Column<bool>(type: "boolean", nullable: false),
                    UltimoAccesso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VersioneToken = table.Column<int>(type: "integer", nullable: false),
                    DipendenteId = table.Column<int>(type: "integer", nullable: true),
                    CreatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utenti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utenti_Dipendenti_DipendenteId",
                        column: x => x.DipendenteId,
                        principalSchema: "gestionale",
                        principalTable: "Dipendenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Utenti_DipendenteId",
                schema: "gestionale",
                table: "Utenti",
                column: "DipendenteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utenti_Email",
                schema: "gestionale",
                table: "Utenti",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Utenti",
                schema: "gestionale");
        }
    }
}
