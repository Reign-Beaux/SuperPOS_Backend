using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IgnoreValueObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleDetails_Sales_SaleId1",
                table: "SaleDetails");

            migrationBuilder.DropIndex(
                name: "IX_SaleDetails_SaleId1",
                table: "SaleDetails");

            migrationBuilder.DropColumn(
                name: "SaleId1",
                table: "SaleDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SaleId1",
                table: "SaleDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleDetails_SaleId1",
                table: "SaleDetails",
                column: "SaleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleDetails_Sales_SaleId1",
                table: "SaleDetails",
                column: "SaleId1",
                principalTable: "Sales",
                principalColumn: "Id");
        }
    }
}
