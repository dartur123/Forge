using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorApprovalRuleEntityType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                table: "ApprovalRules",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ApprovalRules",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRules_RequiredRoleId",
                table: "ApprovalRules",
                column: "RequiredRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRules_Roles_RequiredRoleId",
                table: "ApprovalRules",
                column: "RequiredRoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRules_Roles_RequiredRoleId",
                table: "ApprovalRules");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRules_RequiredRoleId",
                table: "ApprovalRules");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ApprovalRules");

            migrationBuilder.AlterColumn<int>(
                name: "EntityType",
                table: "ApprovalRules",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
