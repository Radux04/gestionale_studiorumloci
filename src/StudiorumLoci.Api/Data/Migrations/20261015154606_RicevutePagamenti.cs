using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudiorumLoci.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class RicevutePagamenti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RicevutaDimensione",
                schema: "gestionale",
                table: "Pagamenti",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RicevutaNome",
                schema: "gestionale",
                table: "Pagamenti",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RicevutaTipo",
                schema: "gestionale",
                table: "Pagamenti",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RicevuteFile",
                schema: "gestionale",
                columns: table => new
                {
                    PagamentoId = table.Column<int>(type: "integer", nullable: false),
                    Contenuto = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RicevuteFile", x => x.PagamentoId);
                    table.ForeignKey(
                        name: "FK_RicevuteFile_Pagamenti_PagamentoId",
                        column: x => x.PagamentoId,
                        principalSchema: "gestionale",
                        principalTable: "Pagamenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RicevuteFile",
                schema: "gestionale");

            migrationBuilder.DropColumn(
                name: "RicevutaDimensione",
                schema: "gestionale",
                table: "Pagamenti");

            migrationBuilder.DropColumn(
                name: "RicevutaNome",
                schema: "gestionale",
                table: "Pagamenti");

            migrationBuilder.DropColumn(
                name: "RicevutaTipo",
                schema: "gestionale",
                table: "Pagamenti");
        }
    }
}
