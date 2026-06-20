using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Forge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityType = table.Column<int>(type: "integer", nullable: false),
                    ThresholdAmountPhp = table.Column<decimal>(type: "numeric", nullable: false),
                    RequiredRoleId = table.Column<int>(type: "integer", nullable: false),
                    SequenceOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillOfMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FinishedGoodMaterialId = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillOfMaterials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyName = table.Column<string>(type: "text", nullable: false),
                    CostingMethod = table.Column<int>(type: "integer", nullable: false),
                    BaseCurrency = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ParentLocationId = table.Column<int>(type: "integer", nullable: true),
                    LocationTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LotNumber = table.Column<string>(type: "text", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    CurrentLocationId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitCostPhp = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalCostPhp = table.Column<decimal>(type: "numeric", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sku = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    UnitOfMeasure = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderNumber = table.Column<string>(type: "text", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmountForeign = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmountPhp = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    LotId = table.Column<int>(type: "integer", nullable: false),
                    FromLocationId = table.Column<int>(type: "integer", nullable: true),
                    ToLocationId = table.Column<int>(type: "integer", nullable: true),
                    JobReference = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitCostPhp = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalCostPhp = table.Column<decimal>(type: "numeric", nullable: false),
                    ReleasedByUserId = table.Column<int>(type: "integer", nullable: false),
                    ReceivedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubconOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderNumber = table.Column<string>(type: "text", nullable: false),
                    SubcontractorId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmountForeign = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmountPhp = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubconOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subcontractors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Specialization = table.Column<string>(type: "text", nullable: true),
                    ContactPerson = table.Column<string>(type: "text", nullable: true),
                    ContactEmail = table.Column<string>(type: "text", nullable: true),
                    ContactPhone = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subcontractors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    ContactPerson = table.Column<string>(type: "text", nullable: true),
                    ContactEmail = table.Column<string>(type: "text", nullable: true),
                    ContactPhone = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillOfMaterialsLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillOfMaterialsId = table.Column<int>(type: "integer", nullable: false),
                    ParentLineId = table.Column<int>(type: "integer", nullable: true),
                    MaterialId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "text", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillOfMaterialsLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillOfMaterialsLines_BillOfMaterialsLines_ParentLineId",
                        column: x => x.ParentLineId,
                        principalTable: "BillOfMaterialsLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillOfMaterialsLines_BillOfMaterials_BillOfMaterialsId",
                        column: x => x.BillOfMaterialsId,
                        principalTable: "BillOfMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseOrderId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitCostForeign = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitCostPhp = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubconOrderLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubconOrderId = table.Column<int>(type: "integer", nullable: false),
                    MaterialSentId = table.Column<int>(type: "integer", nullable: false),
                    QuantitySent = table.Column<decimal>(type: "numeric", nullable: false),
                    ExpectedOutputMaterialId = table.Column<int>(type: "integer", nullable: false),
                    ExpectedOutputQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    ProcessingCostForeign = table.Column<decimal>(type: "numeric", nullable: false),
                    ProcessingCostPhp = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubconOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubconOrderLines_SubconOrders_SubconOrderId",
                        column: x => x.SubconOrderId,
                        principalTable: "SubconOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterialsLines_BillOfMaterialsId",
                table: "BillOfMaterialsLines",
                column: "BillOfMaterialsId");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterialsLines_ParentLineId",
                table: "BillOfMaterialsLines",
                column: "ParentLineId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_PurchaseOrderId",
                table: "PurchaseOrderLines",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SubconOrderLines_SubconOrderId",
                table: "SubconOrderLines",
                column: "SubconOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalRules");

            migrationBuilder.DropTable(
                name: "BillOfMaterialsLines");

            migrationBuilder.DropTable(
                name: "CompanySettings");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "LocationTypes");

            migrationBuilder.DropTable(
                name: "Lots");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "PurchaseOrderLines");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "SubconOrderLines");

            migrationBuilder.DropTable(
                name: "Subcontractors");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "BillOfMaterials");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "SubconOrders");
        }
    }
}
