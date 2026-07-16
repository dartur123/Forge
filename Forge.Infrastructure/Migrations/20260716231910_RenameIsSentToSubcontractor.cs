using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameIsSentToSubcontractor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSentToSupplier",
                table: "SubconOrders",
                newName: "IsSentToSubcontractor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSentToSubcontractor",
                table: "SubconOrders",
                newName: "IsSentToSupplier");
        }
    }
}
