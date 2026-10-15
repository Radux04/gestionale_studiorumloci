using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudiorumLoci.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SediDipendenti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DipendenteSede",
                schema: "gestionale",
                columns: table => new
                {
                    DipendentiId = table.Column<int>(type: "integer", nullable: false),
                    SediId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DipendenteSede", x => new { x.DipendentiId, x.SediId });
                    table.ForeignKey(
                        name: "FK_DipendenteSede_Dipendenti_DipendentiId",
                        column: x => x.DipendentiId,
                        principalSchema: "gestionale",
                        principalTable: "Dipendenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DipendenteSede_Sedi_SediId",
                        column: x => x.SediId,
                        principalSchema: "gestionale",
                        principalTable: "Sedi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DipendenteSede_SediId",
                schema: "gestionale",
                table: "DipendenteSede",
                column: "SediId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DipendenteSede",
                schema: "gestionale");
        }
    }
}
