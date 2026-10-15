using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudiorumLoci.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class StoricoContattiACascata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interazioni_Genitori_GenitoreId",
                schema: "gestionale",
                table: "Interazioni");

            migrationBuilder.DropForeignKey(
                name: "FK_Interazioni_Lead_LeadId",
                schema: "gestionale",
                table: "Interazioni");

            migrationBuilder.AddForeignKey(
                name: "FK_Interazioni_Genitori_GenitoreId",
                schema: "gestionale",
                table: "Interazioni",
                column: "GenitoreId",
                principalSchema: "gestionale",
                principalTable: "Genitori",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Interazioni_Lead_LeadId",
                schema: "gestionale",
                table: "Interazioni",
                column: "LeadId",
                principalSchema: "gestionale",
                principalTable: "Lead",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interazioni_Genitori_GenitoreId",
                schema: "gestionale",
                table: "Interazioni");

            migrationBuilder.DropForeignKey(
                name: "FK_Interazioni_Lead_LeadId",
                schema: "gestionale",
                table: "Interazioni");

            migrationBuilder.AddForeignKey(
                name: "FK_Interazioni_Genitori_GenitoreId",
                schema: "gestionale",
                table: "Interazioni",
                column: "GenitoreId",
                principalSchema: "gestionale",
                principalTable: "Genitori",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Interazioni_Lead_LeadId",
                schema: "gestionale",
                table: "Interazioni",
                column: "LeadId",
                principalSchema: "gestionale",
                principalTable: "Lead",
                principalColumn: "Id");
        }
    }
}
