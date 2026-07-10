using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IntroduceBillOfMaterialAndLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillOfMaterialsLines_BillOfMaterialsLines_ParentLineId",
                table: "BillOfMaterialsLines");

            migrationBuilder.DropIndex(
                name: "IX_BillOfMaterialsLines_ParentLineId",
                table: "BillOfMaterialsLines");

            migrationBuilder.DropColumn(
                name: "ParentLineId",
                table: "BillOfMaterialsLines");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "BillOfMaterialsLines");

            migrationBuilder.DropColumn(
                name: "FinishedGoodMaterialId",
                table: "BillOfMaterials");

            migrationBuilder.DropColumn(name: "Version", table: "BillOfMaterials");
            migrationBuilder.AddColumn<int>(name: "OutputMaterialId", table: "BillOfMaterials",
                type: "integer", nullable: false, defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "BillOfMaterials",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<string>(
                name: "BillOfMaterialStatus",
                table: "BillOfMaterials",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterialsLines_MaterialId",
                table: "BillOfMaterialsLines",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterials_OutputMaterialId",
                table: "BillOfMaterials",
                column: "OutputMaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillOfMaterials_Materials_OutputMaterialId",
                table: "BillOfMaterials",
                column: "OutputMaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BillOfMaterialsLines_Materials_MaterialId",
                table: "BillOfMaterialsLines",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillOfMaterials_Materials_OutputMaterialId",
                table: "BillOfMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_BillOfMaterialsLines_Materials_MaterialId",
                table: "BillOfMaterialsLines");

            migrationBuilder.DropIndex(
                name: "IX_BillOfMaterialsLines_MaterialId",
                table: "BillOfMaterialsLines");

            migrationBuilder.DropIndex(
                name: "IX_BillOfMaterials_OutputMaterialId",
                table: "BillOfMaterials");

            migrationBuilder.DropColumn(
                name: "BillOfMaterialStatus",
                table: "BillOfMaterials");

            migrationBuilder.DropColumn(name: "OutputMaterialId", table: "BillOfMaterials");
            migrationBuilder.AddColumn<int>(name: "Version", table: "BillOfMaterials",
                type: "integer", nullable: false, defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentLineId",
                table: "BillOfMaterialsLines",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                table: "BillOfMaterialsLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "BillOfMaterials",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "FinishedGoodMaterialId",
                table: "BillOfMaterials",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterialsLines_ParentLineId",
                table: "BillOfMaterialsLines",
                column: "ParentLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillOfMaterialsLines_BillOfMaterialsLines_ParentLineId",
                table: "BillOfMaterialsLines",
                column: "ParentLineId",
                principalTable: "BillOfMaterialsLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
