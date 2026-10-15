using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProfessorObservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DispatchedAt",
                table: "Trucks",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdempotencyKey",
                table: "Invoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                table: "Clients",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RouteSettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TruckPlate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DispatchedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    SettledAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    SettledBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Observation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InvoicesCount = table.Column<int>(type: "integer", nullable: false),
                    SalesTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpectedCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CountedCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CashDifference = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MissingGoodsValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteSettlements", x => x.Id);
                    table.CheckConstraint("CK_RouteSettlements_CountedCash_NotNegative", "\"CountedCash\" >= 0");
                    table.ForeignKey(
                        name: "FK_RouteSettlements_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RouteSettlementLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    UnitsPerBox = table.Column<int>(type: "integer", nullable: false),
                    PriceUnit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpectedUnits = table.Column<int>(type: "integer", nullable: false),
                    CountedUnits = table.Column<int>(type: "integer", nullable: false),
                    DifferenceUnits = table.Column<int>(type: "integer", nullable: false),
                    DifferenceValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteSettlementLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteSettlementLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteSettlementLines_RouteSettlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "RouteSettlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("00000300-0000-4000-8000-000000000001"),
                column: "CreditLimit",
                value: null);

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("00000300-0000-4000-8000-000000000002"),
                column: "CreditLimit",
                value: null);

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("00000300-0000-4000-8000-000000000003"),
                column: "CreditLimit",
                value: null);

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("00000300-0000-4000-8000-000000000004"),
                column: "CreditLimit",
                value: null);

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("00000300-0000-4000-8000-000000000005"),
                column: "CreditLimit",
                value: null);

            migrationBuilder.UpdateData(
                table: "Trucks",
                keyColumn: "Id",
                keyValue: new Guid("00000200-0000-4000-8000-000000000001"),
                column: "DispatchedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Trucks",
                keyColumn: "Id",
                keyValue: new Guid("00000200-0000-4000-8000-000000000002"),
                column: "DispatchedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Trucks",
                keyColumn: "Id",
                keyValue: new Guid("00000200-0000-4000-8000-000000000003"),
                column: "DispatchedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Trucks",
                keyColumn: "Id",
                keyValue: new Guid("00000200-0000-4000-8000-000000000004"),
                column: "DispatchedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Trucks",
                keyColumn: "Id",
                keyValue: new Guid("00000200-0000-4000-8000-000000000005"),
                column: "DispatchedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Trucks",
                keyColumn: "Id",
                keyValue: new Guid("00000200-0000-4000-8000-000000000006"),
                column: "DispatchedAt",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_IdempotencyKey",
                table: "Invoices",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Clients_CreditLimit_NotNegative",
                table: "Clients",
                sql: "\"CreditLimit\" IS NULL OR \"CreditLimit\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_RouteSettlementLines_ProductId",
                table: "RouteSettlementLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteSettlementLines_SettlementId",
                table: "RouteSettlementLines",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteSettlements_TruckId_SettledAt",
                table: "RouteSettlements",
                columns: new[] { "TruckId", "SettledAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RouteSettlementLines");

            migrationBuilder.DropTable(
                name: "RouteSettlements");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_IdempotencyKey",
                table: "Invoices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Clients_CreditLimit_NotNegative",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "DispatchedAt",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CreditLimit",
                table: "Clients");
        }
    }
}
