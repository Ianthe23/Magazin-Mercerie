using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace magazinmercerie.Migrations
{
    /// <inheritdoc />
    public partial class IgnoreComputedProduseProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produse_Comenzi_ComandaId",
                table: "Produse");

            migrationBuilder.DropIndex(
                name: "IX_Produse_ComandaId",
                table: "Produse");

            migrationBuilder.DropColumn(
                name: "ComandaId",
                table: "Produse");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ComandaId",
                table: "Produse",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produse_ComandaId",
                table: "Produse",
                column: "ComandaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Produse_Comenzi_ComandaId",
                table: "Produse",
                column: "ComandaId",
                principalTable: "Comenzi",
                principalColumn: "Id");
        }
    }
}
