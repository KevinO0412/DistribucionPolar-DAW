using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TruckSettlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TruckSettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TruckPlate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TripStartedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    SettledAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    SettledBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Observation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpectedCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CountedCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CashDifference = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpectedMerchandiseValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CountedMerchandiseValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MerchandiseDifferenceValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckSettlements", x => x.Id);
                    table.CheckConstraint("CK_TruckSettlements_CountedCash_NotNegative", "\"CountedCash\" >= 0");
                    table.ForeignKey(
                        name: "FK_TruckSettlements_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TruckSettlementLines",
                columns: table => new
                {
                    SettlementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SKU = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UnitsPerBox = table.Column<int>(type: "integer", nullable: false),
                    PriceUnit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpectedUnits = table.Column<int>(type: "integer", nullable: false),
                    CountedUnits = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckSettlementLines", x => new { x.SettlementId, x.ProductId });
                    table.CheckConstraint("CK_TruckSettlementLines_Units_NotNegative", "\"ExpectedUnits\" >= 0 AND \"CountedUnits\" >= 0");
                    table.ForeignKey(
                        name: "FK_TruckSettlementLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TruckSettlementLines_TruckSettlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "TruckSettlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TruckSettlementLines_ProductId",
                table: "TruckSettlementLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TruckSettlements_TruckId_SettledAt",
                table: "TruckSettlements",
                columns: new[] { "TruckId", "SettledAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TruckSettlementLines");

            migrationBuilder.DropTable(
                name: "TruckSettlements");
        }
    }
}
