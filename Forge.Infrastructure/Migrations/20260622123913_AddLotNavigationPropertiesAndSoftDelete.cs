using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLotNavigationPropertiesAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Suppliers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Materials",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Lots",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Lots_CurrentLocationId",
                table: "Lots",
                column: "CurrentLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Lots_MaterialId",
                table: "Lots",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Lots_SupplierId",
                table: "Lots",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lots_Locations_CurrentLocationId",
                table: "Lots",
                column: "CurrentLocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lots_Materials_MaterialId",
                table: "Lots",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lots_Suppliers_SupplierId",
                table: "Lots",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lots_Locations_CurrentLocationId",
                table: "Lots");

            migrationBuilder.DropForeignKey(
                name: "FK_Lots_Materials_MaterialId",
                table: "Lots");

            migrationBuilder.DropForeignKey(
                name: "FK_Lots_Suppliers_SupplierId",
                table: "Lots");

            migrationBuilder.DropIndex(
                name: "IX_Lots_CurrentLocationId",
                table: "Lots");

            migrationBuilder.DropIndex(
                name: "IX_Lots_MaterialId",
                table: "Lots");

            migrationBuilder.DropIndex(
                name: "IX_Lots_SupplierId",
                table: "Lots");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Lots");
        }
    }
}
