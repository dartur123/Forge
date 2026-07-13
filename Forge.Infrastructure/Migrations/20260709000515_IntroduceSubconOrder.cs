using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IntroduceSubconOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAmountForeign",
                table: "SubconOrders");

            migrationBuilder.DropColumn(
                name: "TotalAmountPhp",
                table: "SubconOrders");

            migrationBuilder.DropColumn(
                name: "ProcessingCostPhp",
                table: "SubconOrderLines");

            migrationBuilder.RenameColumn(
                name: "MaterialSentId",
                table: "SubconOrderLines",
                newName: "MaterialId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SubconOrders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "SubconOrders",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<bool>(
                name: "IsSentToSupplier",
                table: "SubconOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "QuantitySent",
                table: "SubconOrderLines",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "ProcessingCostForeign",
                table: "SubconOrderLines",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExpectedOutputQuantity",
                table: "SubconOrderLines",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.CreateIndex(
                name: "IX_SubconOrders_CreatedByUserId",
                table: "SubconOrders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubconOrders_SubcontractorId",
                table: "SubconOrders",
                column: "SubcontractorId");

            migrationBuilder.CreateIndex(
                name: "IX_SubconOrderLines_ExpectedOutputMaterialId",
                table: "SubconOrderLines",
                column: "ExpectedOutputMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_SubconOrderLines_MaterialId",
                table: "SubconOrderLines",
                column: "MaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubconOrderLines_Materials_ExpectedOutputMaterialId",
                table: "SubconOrderLines",
                column: "ExpectedOutputMaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubconOrderLines_Materials_MaterialId",
                table: "SubconOrderLines",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubconOrders_Subcontractors_SubcontractorId",
                table: "SubconOrders",
                column: "SubcontractorId",
                principalTable: "Subcontractors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubconOrders_Users_CreatedByUserId",
                table: "SubconOrders",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubconOrderLines_Materials_ExpectedOutputMaterialId",
                table: "SubconOrderLines");

            migrationBuilder.DropForeignKey(
                name: "FK_SubconOrderLines_Materials_MaterialId",
                table: "SubconOrderLines");

            migrationBuilder.DropForeignKey(
                name: "FK_SubconOrders_Subcontractors_SubcontractorId",
                table: "SubconOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SubconOrders_Users_CreatedByUserId",
                table: "SubconOrders");

            migrationBuilder.DropIndex(
                name: "IX_SubconOrders_CreatedByUserId",
                table: "SubconOrders");

            migrationBuilder.DropIndex(
                name: "IX_SubconOrders_SubcontractorId",
                table: "SubconOrders");

            migrationBuilder.DropIndex(
                name: "IX_SubconOrderLines_ExpectedOutputMaterialId",
                table: "SubconOrderLines");

            migrationBuilder.DropIndex(
                name: "IX_SubconOrderLines_MaterialId",
                table: "SubconOrderLines");

            migrationBuilder.DropColumn(
                name: "IsSentToSupplier",
                table: "SubconOrders");

            migrationBuilder.RenameColumn(
                name: "MaterialId",
                table: "SubconOrderLines",
                newName: "MaterialSentId");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "SubconOrders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "SubconOrders",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmountForeign",
                table: "SubconOrders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmountPhp",
                table: "SubconOrders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "QuantitySent",
                table: "SubconOrderLines",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "ProcessingCostForeign",
                table: "SubconOrderLines",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExpectedOutputQuantity",
                table: "SubconOrderLines",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AddColumn<decimal>(
                name: "ProcessingCostPhp",
                table: "SubconOrderLines",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
