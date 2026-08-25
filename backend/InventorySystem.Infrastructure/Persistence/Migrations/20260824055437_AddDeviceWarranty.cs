using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceWarranty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "WarrantyEndDate",
                table: "Devices",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarrantyNote",
                table: "Devices",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarrantyProvider",
                table: "Devices",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "WarrantyStartDate",
                table: "Devices",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_WarrantyEndDate",
                table: "Devices",
                column: "WarrantyEndDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Devices_WarrantyEndDate",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "WarrantyEndDate",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "WarrantyNote",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "WarrantyProvider",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "WarrantyStartDate",
                table: "Devices");
        }
    }
}
