using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudiorumLoci.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Sedi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SedeId",
                schema: "gestionale",
                table: "Studenti",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Sedi",
                schema: "gestionale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Indirizzo = table.Column<string>(type: "text", nullable: true),
                    Cap = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Comune = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Provincia = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Regione = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Attiva = table.Column<bool>(type: "boolean", nullable: false),
                    CreatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificatoIl = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sedi", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Studenti_SedeId",
                schema: "gestionale",
                table: "Studenti",
                column: "SedeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Studenti_Sedi_SedeId",
                schema: "gestionale",
                table: "Studenti",
                column: "SedeId",
                principalSchema: "gestionale",
                principalTable: "Sedi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Studenti_Sedi_SedeId",
                schema: "gestionale",
                table: "Studenti");

            migrationBuilder.DropTable(
                name: "Sedi",
                schema: "gestionale");

            migrationBuilder.DropIndex(
                name: "IX_Studenti_SedeId",
                schema: "gestionale",
                table: "Studenti");

            migrationBuilder.DropColumn(
                name: "SedeId",
                schema: "gestionale",
                table: "Studenti");
        }
    }
}
