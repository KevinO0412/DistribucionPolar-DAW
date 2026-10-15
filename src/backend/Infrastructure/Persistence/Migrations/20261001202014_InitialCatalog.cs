using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Deposit = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Brand = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SKU = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PriceBox = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceUnit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitsPerBox = table.Column<int>(type: "integer", nullable: false),
                    StockUnits = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MinStock = table.Column<int>(type: "integer", nullable: false),
                    MaxStock = table.Column<int>(type: "integer", nullable: false),
                    IsReturnable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EmptyGroupKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.CheckConstraint("CK_Products_CostPrice_NotNegative", "\"CostPrice\" >= 0");
                    table.CheckConstraint("CK_Products_MaxStock_GreaterThan_MinStock", "\"MaxStock\" > \"MinStock\"");
                    table.CheckConstraint("CK_Products_PriceBox_Positive", "\"PriceBox\" > 0");
                    table.CheckConstraint("CK_Products_PriceUnit_Positive", "\"PriceUnit\" > 0");
                    table.CheckConstraint("CK_Products_StockUnits_NotNegative", "\"StockUnits\" >= 0");
                    table.CheckConstraint("CK_Products_UnitsPerBox_Positive", "\"UnitsPerBox\" > 0");
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Deposit", "Description", "LastModifiedAt", "Name" },
                values: new object[,]
                {
                    { new Guid("a1111111-1111-4111-8111-111111111111"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Depósito #1 - Alimentos", "Víveres, granos, harinas, café, enlatados y alimentos para mascotas", null, "Alimentos" },
                    { new Guid("b2222222-2222-4222-8222-222222222222"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Depósito #2 - Bebidas", "Cervezas, maltas, refrescos y aguas; incluye envases retornables", null, "Bebidas" },
                    { new Guid("c3333333-3333-4333-8333-333333333333"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Depósito #3 - Jabones/P&G", "Detergentes, limpieza del hogar y cuidado personal", null, "Jabones/P&G" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[] { new Guid("00000000-0000-4000-8000-000000000001"), "Polar Light", new Guid("b2222222-2222-4222-8222-222222222222"), 18.40m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "RET_222ML_X36", true, true, null, 1440, 180, "POLAR LIGHT RET 222MLx36UN", 23.0m, 0.64m, "BEB-POL-0001", 252, 36 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-4000-8000-000000000002"), "Polar Light", new Guid("b2222222-2222-4222-8222-222222222222"), 16.32m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "POLAR LIGHT LAT 250MLx24UN", 20.4m, 0.85m, "BEB-POL-0002", 336, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000003"), "Polar Light", new Guid("b2222222-2222-4222-8222-222222222222"), 19.22m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "POLAR LIGHT LAT SLEEK 355MLx24UN", 24.02m, 1.0m, "BEB-POL-0003", 504, 24 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[] { new Guid("00000000-0000-4000-8000-000000000004"), "Solera Light", new Guid("b2222222-2222-4222-8222-222222222222"), 18.40m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "RET_222ML_X36", true, true, null, 1440, 180, "SOLERA LIGHT RET 222MLx36UN", 23.0m, 0.64m, "BEB-SOL-0004", 1008, 36 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[] { new Guid("00000000-0000-4000-8000-000000000005"), "Solera Light", new Guid("b2222222-2222-4222-8222-222222222222"), 16.32m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "SOLERA LIGHT LAT 250MLx24UN", 20.4m, 0.85m, "BEB-SOL-0005", 840, 24 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[] { new Guid("00000000-0000-4000-8000-000000000006"), "Maltin Polar", new Guid("b2222222-2222-4222-8222-222222222222"), 13.90m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "RET_222ML_X36", true, true, null, 1440, 180, "MALTIN POLAR RET 222MLx36UN", 17.38m, 0.48m, "BEB-MAL-0006", 1512, 36 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-4000-8000-000000000007"), "Maltin Polar", new Guid("b2222222-2222-4222-8222-222222222222"), 14.06m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "MALTIN POLAR LAT 250MLx24UN", 17.57m, 0.73m, "BEB-MAL-0007", 96, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000008"), "Maltin Polar", new Guid("b2222222-2222-4222-8222-222222222222"), 7.87m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "MALTIN POLAR PET 1.5 Lx6UN", 9.84m, 1.64m, "BEB-MAL-0008", 66, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000009"), "Maltin Polar", new Guid("b2222222-2222-4222-8222-222222222222"), 6.24m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "MALTIN POLAR NR 250MLx12UN", 7.8m, 0.65m, "BEB-MAL-0009", 216, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000010"), "Maltin Polar", new Guid("b2222222-2222-4222-8222-222222222222"), 17.11m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "MALTIN POLAR LAT SLEEK 355MLx24UN", 21.39m, 0.89m, "BEB-MAL-0010", 600, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000011"), "Solera", new Guid("b2222222-2222-4222-8222-222222222222"), 18.44m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "SOLERA BOHEMIA LATA 250MLX24UN", 23.05m, 0.96m, "BEB-SOL-0011", 768, 24 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[] { new Guid("00000000-0000-4000-8000-000000000012"), "Solera", new Guid("b2222222-2222-4222-8222-222222222222"), 20.70m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "RET_222ML_X36", true, true, null, 1440, 180, "SOLERA RET 222MLx36UN", 25.87m, 0.72m, "BEB-SOL-0012", 1404, 36 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-4000-8000-000000000013"), "Sangria Caroreña", new Guid("b2222222-2222-4222-8222-222222222222"), 30.87m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "SANGRIA CAROREÑA TINTA 1,75Lx6UN", 38.59m, 6.43m, "BEB-SAN-0013", 6, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000014"), "Sangria Caroreña", new Guid("b2222222-2222-4222-8222-222222222222"), 30.87m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "SANGRIA CAROREÑA BLANCA 1,75Lx6UN", 38.59m, 6.43m, "BEB-SAN-0014", 48, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000015"), "Sangria Caroreña", new Guid("b2222222-2222-4222-8222-222222222222"), 30.87m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "SANGRIA CAROREÑA ROSADA 1,75Lx6UN", 38.59m, 6.43m, "BEB-SAN-0015", 90, 6 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[] { new Guid("00000000-0000-4000-8000-000000000016"), "Sangria Caroreña", new Guid("b2222222-2222-4222-8222-222222222222"), 23.04m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "RET_222ML_X36", true, true, null, 1440, 180, "SANGRIA CAROREÑA VERANO RET222MLx36UN", 28.8m, 0.8m, "BEB-SAN-0016", 792, 36 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-4000-8000-000000000017"), "Sangria Caroreña", new Guid("b2222222-2222-4222-8222-222222222222"), 24.32m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "SANGRIA CAROREÑA LATA 250MLx24UN", 30.4m, 1.27m, "BEB-SAN-0017", 696, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000018"), "Sangria Caroreña", new Guid("b2222222-2222-4222-8222-222222222222"), 14.73m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "SANGRIA CAROREÑA VERANO MOJITO LAT355x12", 18.41m, 1.53m, "BEB-SAN-0018", 432, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000019"), "Sangria La Que Manda", new Guid("b2222222-2222-4222-8222-222222222222"), 22.90m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "SANGRIA LA QUE MANDA TINTA 1,75LX6UN", 28.63m, 4.77m, "BEB-SAN-0019", 258, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000020"), "Sangria La Que Manda", new Guid("b2222222-2222-4222-8222-222222222222"), 18.43m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "SANGRIA LA QUE MANDA BLANCA LATA 250MLX24UN", 23.04m, 0.96m, "BEB-SAN-0020", 120, 24 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-4000-8000-000000000021"), "Polar Pilsen", new Guid("b2222222-2222-4222-8222-222222222222"), 18.40m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "RET_222ML_X36", true, true, null, 1440, 180, "POLAR PILSEN RET 222MLx36UN", 23.0m, 0.64m, "BEB-POL-0021", 432, 36 },
                    { new Guid("00000000-0000-4000-8000-000000000022"), "Polar Pilsen", new Guid("b2222222-2222-4222-8222-222222222222"), 16.43m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "RET_POLAR_PILSEN_330ML_X24", true, true, null, 960, 120, "POLAR PILSEN RET 330MLx24UN", 20.54m, 0.86m, "BEB-POL-0022", 456, 24 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-4000-8000-000000000023"), "Polar Pilsen", new Guid("b2222222-2222-4222-8222-222222222222"), 16.32m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "POLAR PILSEN LAT 250MLx24UN", 20.4m, 0.85m, "BEB-POL-0023", 624, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000024"), "Polar Pilsen", new Guid("b2222222-2222-4222-8222-222222222222"), 19.22m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "POLAR PILSEN LAT SLEEK 355MLx24UN", 24.02m, 1.0m, "BEB-POL-0024", 792, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000025"), "Minalba", new Guid("b2222222-2222-4222-8222-222222222222"), 12.70m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "MINALBA AGUA PET 355MLx24UN", 15.87m, 0.66m, "BEB-MIN-0025", 960, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000026"), "Minalba", new Guid("b2222222-2222-4222-8222-222222222222"), 16.11m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "MINALBA AGUA PET TR 600MLx24UN", 20.14m, 0.84m, "BEB-MIN-0026", 48, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000027"), "Minalba", new Guid("b2222222-2222-4222-8222-222222222222"), 7.35m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 80, 10, "MINALBA AGUA PET S/G 5Lx2UN", 9.19m, 4.59m, "BEB-MIN-0027", 18, 2 },
                    { new Guid("00000000-0000-4000-8000-000000000028"), "Minalba", new Guid("b2222222-2222-4222-8222-222222222222"), 10.96m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "MINALBA SPARKLYN 500MLX12UN", 13.7m, 1.14m, "BEB-MIN-0028", 192, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000029"), "Minalba", new Guid("b2222222-2222-4222-8222-222222222222"), 14.96m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "MINALBA AGUA PET S/G 1,5Lx12UN", 18.7m, 1.56m, "BEB-MIN-0029", 276, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000030"), "Gatorade", new Guid("b2222222-2222-4222-8222-222222222222"), 16.99m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "GATORADE PET 500MLx12UN", 21.24m, 1.77m, "BEB-GAT-0030", 360, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000031"), "Rockstar", new Guid("b2222222-2222-4222-8222-222222222222"), 16.96m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "ROCKSTAR LATA 355MLx24UN", 21.2m, 0.88m, "BEB-ROC-0031", 888, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000032"), "Yukery", new Guid("b2222222-2222-4222-8222-222222222222"), 19.00m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "YUKERY NARANJADA S/A PET 1,5Lx6UN", 23.75m, 3.96m, "BEB-YUK-0032", 264, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000033"), "Yukery", new Guid("b2222222-2222-4222-8222-222222222222"), 19.00m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "YUKERY NARANJADA PET 1,5Lx6UN", 23.75m, 3.96m, "BEB-YUK-0033", 36, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000034"), "Yuk.", new Guid("b2222222-2222-4222-8222-222222222222"), 8.43m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "YUK. DURAZNO C.P. BOT 250MLX12UN", 10.54m, 0.88m, "BEB-YUK-0034", 156, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000035"), "Yuky-Pak", new Guid("b2222222-2222-4222-8222-222222222222"), 17.35m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "YUKY-PAK MANZANA LD 250MLx24UN", 21.69m, 0.9m, "BEB-YUK-0035", 480, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000036"), "Pepsi / Sabores", new Guid("b2222222-2222-4222-8222-222222222222"), 5.60m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "SABORES 2LX6UN", 7.0m, 1.17m, "BEB-SAB-0036", 162, 6 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[] { new Guid("00000000-0000-4000-8000-000000000037"), "Pepsi / Sabores", new Guid("b2222222-2222-4222-8222-222222222222"), 3.60m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PEPSI_125L_X6", true, true, null, 240, 30, "PEPSI/SABORES 1,25X6UN", 4.5m, 0.75m, "BEB-PEP-0037", 204, 6 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-4000-8000-000000000038"), "Pepsi / Sabores", new Guid("b2222222-2222-4222-8222-222222222222"), 3.61m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "PEPSI/SABORES 1LX6UN", 4.51m, 0.75m, "BEB-PEP-0038", 246, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000039"), "Pepsi / Sabores", new Guid("b2222222-2222-4222-8222-222222222222"), 17.61m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "PEPSI/SABORES LATA 355X24UN", 22.01m, 0.92m, "BEB-PEP-0039", 72, 24 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[] { new Guid("00000000-0000-4000-8000-000000000040"), "Pepsi / Sabores", new Guid("b2222222-2222-4222-8222-222222222222"), 8.80m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PEPSI_350ML_X24", true, true, null, 960, 120, "PEPSI/SABORES 350X24UN", 11.0m, 0.46m, "BEB-PEP-0040", 240, 24 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-4000-8000-000000000041"), "Minalba", new Guid("b2222222-2222-4222-8222-222222222222"), 20.80m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "MINALBA SPARKLING SODA LAT355MLx24UN", 26.0m, 1.08m, "BEB-MIN-0041", 408, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000042"), "Pan", new Guid("a1111111-1111-4111-8111-111111111111"), 21.92m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 800, 100, "PAN HARINA MAIZ AMARILLA 1KGx20UN BOPP", 27.4m, 1.37m, "ALI-PAN-0042", 480, 20 },
                    { new Guid("00000000-0000-4000-8000-000000000043"), "Pan", new Guid("a1111111-1111-4111-8111-111111111111"), 21.92m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 800, 100, "PAN MEZCLA MAIZ BLANCO Y ARROZ 1KGx20UN", 27.4m, 1.37m, "ALI-PAN-0043", 620, 20 },
                    { new Guid("00000000-0000-4000-8000-000000000044"), "Pan", new Guid("a1111111-1111-4111-8111-111111111111"), 19.84m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 800, 100, "PAN HPM BLANCO GLUTEN FREE 1KGX20UN VE", 24.8m, 1.24m, "ALI-PAN-0044", 760, 20 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "UnitsPerBox" },
                values: new object[] { new Guid("00000000-0000-4000-8000-000000000045"), "Pan", new Guid("a1111111-1111-4111-8111-111111111111"), 22.83m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PAN MEZCLA PARA CACHAPAS 500GRx12UN", 28.54m, 2.38m, "ALI-PAN-0045", 12 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-4000-8000-000000000046"), "Pan", new Guid("a1111111-1111-4111-8111-111111111111"), 22.83m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PAN SEMILLAS NUTRITIVAS 500Gx12UN", 28.54m, 2.38m, "ALI-PAN-0046", 84, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000047"), "Pan", new Guid("a1111111-1111-4111-8111-111111111111"), 22.83m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PAN AREPITAS DULCE 500Gx12UN", 28.54m, 2.38m, "ALI-PAN-0047", 168, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000048"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 38.40m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "PRIMOR ARROZ PERLADO 900Gx24UN", 48m, 2m, "ALI-PRI-0048", 504, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000049"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 27.26m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "PRIMOR ARROZ TRADICIONAL 900Gx24UN", 34.08m, 1.42m, "ALI-PRI-0049", 672, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000050"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 28.42m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "PRIMOR ARROZ CLÁSICO SUPERIOR 900Gx24UN", 35.52m, 1.48m, "ALI-PRI-0050", 840, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000051"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 17.57m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PRIMOR PASTA LARGA LINGUINI 1KGx12UN", 21.96m, 1.83m, "ALI-PRI-0051", 504, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000052"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 19.20m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PRIMOR PASTA CORTA DEDALES 1KGx12UN", 24m, 2m, "ALI-PRI-0052", 48, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000053"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 19.20m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PRIMOR PASTA CORTA TORNILLO 1KGx12UN", 24m, 2m, "ALI-PRI-0053", 132, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000054"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 19.20m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PRIMOR PASTA CORTA PLUMITAS 1KGx12UN", 24m, 2m, "ALI-PRI-0054", 216, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000055"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 9.70m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PRIMOR PASTA CORTA DEDALES 500Gx12UN", 12.12m, 1.01m, "ALI-PRI-0055", 300, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000056"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 9.70m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PRIMOR PASTA CORTA PLUMITAS 500Gx12UN", 12.12m, 1.01m, "ALI-PRI-0056", 384, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000057"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 8.83m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PRIMOR PASTA LARGA VERMICELL 500Gx12UN", 11.04m, 0.92m, "ALI-PRI-0057", 468, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000058"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 17.57m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PRIMOR PASTA L VERMICELLI 1KGx12UN NR", 21.96m, 1.83m, "ALI-PRI-0058", 12, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000059"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 15.94m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PRIMOR EXTRA ESPECIAL VERMICELLI 1KGx12U", 19.92m, 1.66m, "ALI-PRI-0059", 96, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000060"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 17.57m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PRIMOR EXTRA ESPECIAL PLUMITAS 1KGx12U", 21.96m, 1.83m, "ALI-PRI-0060", 180, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000061"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 17.57m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PRIMOR EXTRA ESPECIAL TORNILLOS 1KGx12U", 21.96m, 1.83m, "ALI-PRI-0061", 264, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000062"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 17.57m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PRIMOR EXTRA ESPECIAL DEDALES 1KGx12U", 21.96m, 1.83m, "ALI-PRI-0062", 348, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000063"), "Ketchup", new Guid("a1111111-1111-4111-8111-111111111111"), 25.83m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "KETCHUP PAMPERO 198 G X 24 UND", 32.29m, 1.35m, "ALI-KET-0063", 864, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000064"), "Pampero", new Guid("a1111111-1111-4111-8111-111111111111"), 38.98m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "PAMPERO KETCHUP 397Gx24UN", 48.72m, 2.03m, "ALI-PAM-0064", 1032, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000065"), "Pampero", new Guid("a1111111-1111-4111-8111-111111111111"), 50.41m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 160, 20, "PAMPERO SALSA BASE TOMATE 4,2KGx4UN CFH", 63.01m, 15.75m, "ALI-PAM-0065", 20, 4 },
                    { new Guid("00000000-0000-4000-8000-000000000066"), "Mavesa", new Guid("a1111111-1111-4111-8111-111111111111"), 20.05m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "MAVESA VINAGRE DE ALCOHOL 1Lx12UN", 25.06m, 2.09m, "ALI-MAV-0066", 144, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000067"), "Mavesa", new Guid("a1111111-1111-4111-8111-111111111111"), 23.83m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "MAVESA VINAGRE 500MLx24UN", 29.79m, 1.24m, "ALI-MAV-0067", 456, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000068"), "Mavesa", new Guid("a1111111-1111-4111-8111-111111111111"), 23.61m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 160, 20, "MAVESA VINAGRE DE ALCOHOL 4Lx4UN CFH", 29.51m, 7.38m, "ALI-MAV-0068", 104, 4 },
                    { new Guid("00000000-0000-4000-8000-000000000069"), "Mavesa", new Guid("a1111111-1111-4111-8111-111111111111"), 24.24m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "MAVESA MARGARINA 1000Gx6UN", 30.3m, 5.05m, "ALI-MAV-0069", 198, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000070"), "Mavesa", new Guid("a1111111-1111-4111-8111-111111111111"), 25.82m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "MAVESA MARGARINA 500Gx12UN", 32.28m, 2.69m, "ALI-MAV-0070", 480, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000071"), "Mavesa", new Guid("a1111111-1111-4111-8111-111111111111"), 28.80m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "MAVESA MARGARINA 250Gx24UN", 36m, 1.5m, "ALI-MAV-0071", 48, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000072"), "Nelly", new Guid("a1111111-1111-4111-8111-111111111111"), 24.58m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "NELLY MARGARINA REDUCIDA CAL 250Gx24UN", 30.72m, 1.28m, "ALI-NEL-0072", 216, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000073"), "Nelly", new Guid("a1111111-1111-4111-8111-111111111111"), 23.90m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "NELLY MARGARINA REDUCIDA CAL 500Gx12UN", 29.88m, 2.49m, "ALI-NEL-0073", 192, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000074"), "Mavesa", new Guid("a1111111-1111-4111-8111-111111111111"), 35.23m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "MAVESA MAYONESA 910Gx6UN", 44.04m, 7.34m, "ALI-MAV-0074", 138, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000075"), "Mavesa", new Guid("a1111111-1111-4111-8111-111111111111"), 37.15m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "MAVESA MAYONESA 445Gx12UN", 46.44m, 3.87m, "ALI-MAV-0075", 360, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000076"), "Mavesa", new Guid("a1111111-1111-4111-8111-111111111111"), 37.25m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "MAVESA MAYONESA 175Gx24UN STOCK", 46.56m, 1.94m, "ALI-MAV-0076", 888, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000077"), "Mavesa", new Guid("a1111111-1111-4111-8111-111111111111"), 92.54m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 160, 20, "MAVESA ADEREZO MAYONESA 3,6KGx4UN CFH", 115.68m, 28.92m, "ALI-MAV-0077", 176, 4 },
                    { new Guid("00000000-0000-4000-8000-000000000078"), "Café Anzotegui", new Guid("a1111111-1111-4111-8111-111111111111"), 29.76m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 1200, 150, "CAFÉ ANZOTEGUI 100X30UN", 37.2m, 1.24m, "ALI-CAF-0078", 180, 30 },
                    { new Guid("00000000-0000-4000-8000-000000000079"), "Café Anzotegui", new Guid("a1111111-1111-4111-8111-111111111111"), 38.72m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 800, 100, "CAFÉ ANZOTEGUI 200X20UN", 48.4m, 2.42m, "ALI-CAF-0079", 260, 20 },
                    { new Guid("00000000-0000-4000-8000-000000000080"), "Café Anzotegui", new Guid("a1111111-1111-4111-8111-111111111111"), 47.84m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 400, 50, "CAFÉ ANZOTEGUI 500X10UN", 59.8m, 5.98m, "ALI-CAF-0080", 200, 10 },
                    { new Guid("00000000-0000-4000-8000-000000000081"), "Café Anzoategui", new Guid("a1111111-1111-4111-8111-111111111111"), 26.40m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 2000, 250, "CAFÉ ANZOATEGUI 50X50UN", 33m, 0.66m, "ALI-CAF-0081", 1350, 50 },
                    { new Guid("00000000-0000-4000-8000-000000000082"), "Buen Café", new Guid("a1111111-1111-4111-8111-111111111111"), 30.00m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 1200, 150, "BUEN CAFÉ 100X30UN", 37.5m, 1.25m, "ALI-BUE-0082", 1020, 30 },
                    { new Guid("00000000-0000-4000-8000-000000000083"), "Buen Café", new Guid("a1111111-1111-4111-8111-111111111111"), 39.36m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 800, 100, "BUEN CAFÉ 200X20UN", 49.2m, 2.46m, "ALI-BUE-0083", 820, 20 },
                    { new Guid("00000000-0000-4000-8000-000000000084"), "Buen Café", new Guid("a1111111-1111-4111-8111-111111111111"), 48.88m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 400, 50, "BUEN CAFÉ 500X10UN", 61.1m, 6.11m, "ALI-BUE-0084", 30, 10 },
                    { new Guid("00000000-0000-4000-8000-000000000085"), "Margarita", new Guid("a1111111-1111-4111-8111-111111111111"), 63.00m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 1400, 175, "MARGARITA ATUN NATURAL 140Gx35UN", 78.75m, 2.25m, "ALI-MAR-0085", 350, 35 },
                    { new Guid("00000000-0000-4000-8000-000000000086"), "Margarita", new Guid("a1111111-1111-4111-8111-111111111111"), 50.88m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "MARGARITA ATUN EN AGUA 170Gx24UN", 63.6m, 2.65m, "ALI-MAR-0086", 408, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000087"), "Margarita", new Guid("a1111111-1111-4111-8111-111111111111"), 69.83m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 1400, 175, "MARGARITA ATUN EN ACEITE 140Gx35UN", 87.29m, 2.49m, "ALI-MAR-0087", 840, 35 },
                    { new Guid("00000000-0000-4000-8000-000000000088"), "Margarita", new Guid("a1111111-1111-4111-8111-111111111111"), 56.79m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "MARGARITA ATUN EN ACEITE 170Gx24UN", 70.99m, 2.96m, "ALI-MAR-0088", 744, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000089"), "Margarita", new Guid("a1111111-1111-4111-8111-111111111111"), 59.02m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "MARGARITA ATUN TROZOS ACEITE 170Gx24UN", 73.78m, 3.07m, "ALI-MAR-0089", 912, 24 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "UnitsPerBox" },
                values: new object[] { new Guid("00000000-0000-4000-8000-000000000090"), "California", new Guid("a1111111-1111-4111-8111-111111111111"), 47.42m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 1400, 175, "CALIFORNIA ATUN DESMENUZADO 140Gx35UN", 59.28m, 1.69m, "ALI-CAL-0090", 35 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-4000-8000-000000000091"), "Margarita", new Guid("a1111111-1111-4111-8111-111111111111"), 48.72m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 1400, 175, "MARGARITA PEPITONA PIC. 140Gx35UN", 60.9m, 1.74m, "ALI-MAR-0091", 245, 35 },
                    { new Guid("00000000-0000-4000-8000-000000000092"), "Margarita", new Guid("a1111111-1111-4111-8111-111111111111"), 17.76m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 800, 100, "MARGARITA SARDINA ACEITE 170Gx20UN", 22.2m, 1.11m, "ALI-MAR-0092", 280, 20 },
                    { new Guid("00000000-0000-4000-8000-000000000093"), "Margarita", new Guid("a1111111-1111-4111-8111-111111111111"), 17.76m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 800, 100, "MARGARITA SARDINA PIC. 170Gx20UN", 22.2m, 1.11m, "ALI-MAR-0093", 420, 20 },
                    { new Guid("00000000-0000-4000-8000-000000000094"), "Margarita", new Guid("a1111111-1111-4111-8111-111111111111"), 17.76m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 800, 100, "MARGARITA SARDINA TOMATE 170Gx20UN", 22.2m, 1.11m, "ALI-MAR-0094", 560, 20 },
                    { new Guid("00000000-0000-4000-8000-000000000095"), "Margarita", new Guid("a1111111-1111-4111-8111-111111111111"), 17.76m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 800, 100, "MARGARITA SARDINA AHUMADA 170Gx20UN", 22.2m, 1.11m, "ALI-MAR-0095", 700, 20 },
                    { new Guid("00000000-0000-4000-8000-000000000096"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 30.91m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "PRIMOR CREMA ARROZ ENRIQUECI 450Gx24UN", 38.64m, 1.61m, "ALI-PRI-0096", 1008, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000097"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 24.58m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PRIMOR CREMA ARROZ ENRIQUECI 900Gx12UN", 30.72m, 2.56m, "ALI-PRI-0097", 48, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000098"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 11.36m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 800, 100, "PRIMOR CREMA ARROZ BOLSA ENRIQ 225Gx20UN", 14.2m, 0.71m, "ALI-PRI-0098", 220, 20 },
                    { new Guid("00000000-0000-4000-8000-000000000099"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 15.87m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 640, 80, "PRIMOR CREMA ARROZ BOLSA ENRIQ 450Gx16UN", 19.84m, 1.24m, "ALI-PRI-0099", 288, 16 },
                    { new Guid("00000000-0000-4000-8000-000000000100"), "Primor", new Guid("a1111111-1111-4111-8111-111111111111"), 18.91m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PRIMOR CREMA ARROZ BOLSA ENRIQ 900Gx12UN", 23.64m, 1.97m, "ALI-PRI-0100", 300, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000101"), "Quaker", new Guid("a1111111-1111-4111-8111-111111111111"), 19.20m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 640, 80, "QUAKER AVENA ORIGINAL BOLSA 400Gx16UN", 24m, 1.5m, "ALI-QUA-0101", 512, 16 },
                    { new Guid("00000000-0000-4000-8000-000000000102"), "Quaker", new Guid("a1111111-1111-4111-8111-111111111111"), 25.92m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "QUAKER AVENA ORIGINAL BOLSA 800Gx12UN", 32.4m, 2.7m, "ALI-QUA-0102", 468, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000103"), "Quaker", new Guid("a1111111-1111-4111-8111-111111111111"), 19.20m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 640, 80, "QUAKER HARINA DE AVENA (BOLS) 400Gx16UN", 24m, 1.5m, "ALI-QUA-0103", 16, 16 },
                    { new Guid("00000000-0000-4000-8000-000000000104"), "Quaker", new Guid("a1111111-1111-4111-8111-111111111111"), 12.16m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 800, 100, "QUAKER AVENA ORIGINAL BOLSA 200Gx20UN", 15.2m, 0.76m, "ALI-QUA-0104", 160, 20 },
                    { new Guid("00000000-0000-4000-8000-000000000105"), "Toddy", new Guid("a1111111-1111-4111-8111-111111111111"), 105.34m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "TODDY BOLSA 1KGx12UN", 131.68m, 10.97m, "ALI-TOD-0105", 180, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000106"), "Toddy", new Guid("a1111111-1111-4111-8111-111111111111"), 131.70m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 320, 40, "TODDY 2KGx8UN", 164.63m, 20.58m, "ALI-TOD-0106", 176, 8 },
                    { new Guid("00000000-0000-4000-8000-000000000107"), "Toddy", new Guid("a1111111-1111-4111-8111-111111111111"), 95.54m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "TODDY BOLSA 400Gx24UN", 119.43m, 4.98m, "ALI-TOD-0107", 696, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000108"), "Toddy", new Guid("a1111111-1111-4111-8111-111111111111"), 47.78m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "TODDY LATA 400Gx12UN", 59.72m, 4.98m, "ALI-TOD-0108", 432, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000109"), "Toddy", new Guid("a1111111-1111-4111-8111-111111111111"), 25.28m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "TODDY ENVASE 200Gx12UN", 31.6m, 2.63m, "ALI-TOD-0109", 516, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000110"), "Toddy", new Guid("a1111111-1111-4111-8111-111111111111"), 40.76m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 1440, 180, "TODDY BOLSA 100GX36UN", 50.95m, 1.42m, "ALI-TOD-0110", 180, 36 },
                    { new Guid("00000000-0000-4000-8000-000000000111"), "Rikesa", new Guid("a1111111-1111-4111-8111-111111111111"), 45.60m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 720, 90, "RIKESA QUESO CHEDDAR PICANTE 200Gx18UN", 57m, 3.17m, "ALI-RIK-0111", 216, 18 },
                    { new Guid("00000000-0000-4000-8000-000000000112"), "Rikesa", new Guid("a1111111-1111-4111-8111-111111111111"), 75.50m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 720, 90, "RIKESA QUESO CHEDDAR SQUEEZE 330Gx18UN", 94.38m, 5.24m, "ALI-RIK-0112", 342, 18 },
                    { new Guid("00000000-0000-4000-8000-000000000113"), "Rikesa", new Guid("a1111111-1111-4111-8111-111111111111"), 49.94m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 720, 90, "RIKESA QUESO ORIGINAL ENRIQ 200Gx18UN", 62.43m, 3.47m, "ALI-RIK-0113", 468, 18 },
                    { new Guid("00000000-0000-4000-8000-000000000114"), "Rikesa", new Guid("a1111111-1111-4111-8111-111111111111"), 48.22m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "RIKESA QUESO ORIGINAL ENRIQ 300Gx12UN", 60.27m, 5.02m, "ALI-RIK-0114", 396, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000115"), "Rikesa", new Guid("a1111111-1111-4111-8111-111111111111"), 48.22m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "RIKESA QUESO TOCINETA ENRIQ 300Gx12UN", 60.27m, 5.02m, "ALI-RIK-0115", 480, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000116"), "Konga", new Guid("a1111111-1111-4111-8111-111111111111"), 37.42m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 4480, 560, "KONGA SABOR LIMON 30GX8X14UN", 46.77m, 0.42m, "ALI-KON-0116", 224, 112 },
                    { new Guid("00000000-0000-4000-8000-000000000117"), "Konga", new Guid("a1111111-1111-4111-8111-111111111111"), 37.42m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 4480, 560, "KONGA SABOR NARANJA 30GX8X14UN", 46.77m, 0.42m, "ALI-KON-0117", 1008, 112 },
                    { new Guid("00000000-0000-4000-8000-000000000118"), "Konga", new Guid("a1111111-1111-4111-8111-111111111111"), 37.42m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 4480, 560, "KONGA SABOR A MORA 30GX8X14UN", 46.77m, 0.42m, "ALI-KON-0118", 1792, 112 },
                    { new Guid("00000000-0000-4000-8000-000000000119"), "Konga", new Guid("a1111111-1111-4111-8111-111111111111"), 37.42m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 4480, 560, "KONGA SABOR PARCHITA 30GX8X14UN", 46.77m, 0.42m, "ALI-KON-0119", 2576, 112 },
                    { new Guid("00000000-0000-4000-8000-000000000120"), "Konga", new Guid("a1111111-1111-4111-8111-111111111111"), 37.42m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 4480, 560, "KONGA SABOR TIZANA 30GX8X14UN", 46.77m, 0.42m, "ALI-KON-0120", 3360, 112 },
                    { new Guid("00000000-0000-4000-8000-000000000121"), "Konga", new Guid("a1111111-1111-4111-8111-111111111111"), 37.42m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 4480, 560, "KONGA SABOR DURAZNO 30GX8X14UN", 46.77m, 0.42m, "ALI-KON-0121", 4144, 112 },
                    { new Guid("00000000-0000-4000-8000-000000000122"), "Konga", new Guid("a1111111-1111-4111-8111-111111111111"), 37.42m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 4480, 560, "KONGA PAPELON CON LIMON 30GX8X14UNI", 46.77m, 0.42m, "ALI-KON-0122", 4928, 112 },
                    { new Guid("00000000-0000-4000-8000-000000000123"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 32.26m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 1920, 240, "LAS LLAVES JABON FF BEBE 160X48UN", 40.32m, 0.84m, "ALI-LAS-0123", 288, 48 },
                    { new Guid("00000000-0000-4000-8000-000000000124"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 30.62m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 1760, 220, "LAS LLAVES JABON TRAD. FLORAL 200Gx44UN", 38.28m, 0.87m, "ALI-LAS-0124", 572, 44 },
                    { new Guid("00000000-0000-4000-8000-000000000125"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 30.82m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 1440, 180, "LAS LLAVES JABON TRAD. FLORAL 250Gx36UN", 38.52m, 1.07m, "ALI-LAS-0125", 720, 36 },
                    { new Guid("00000000-0000-4000-8000-000000000126"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 17.79m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 640, 80, "LAS LLAVES DETERGENTE LIMON 400Gx16UN", 22.24m, 1.39m, "ALI-LAS-0126", 432, 16 },
                    { new Guid("00000000-0000-4000-8000-000000000127"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 24.32m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 400, 50, "LAS LLAVES DETERGENTE P. LI 900Gx10UN", 30.4m, 3.04m, "ALI-LAS-0127", 340, 10 },
                    { new Guid("00000000-0000-4000-8000-000000000128"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 17.79m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 640, 80, "LAS LLAVES DETERGENTE FLORAL 400Gx16UN", 22.24m, 1.39m, "ALI-LAS-0128", 656, 16 },
                    { new Guid("00000000-0000-4000-8000-000000000129"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 24.32m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 400, 50, "LAS LLAVES DETERGENTE FLORAL 900Gx10UN", 30.4m, 3.04m, "ALI-LAS-0129", 30, 10 },
                    { new Guid("00000000-0000-4000-8000-000000000130"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 24.32m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 400, 50, "LAS LLAVES DETERGENTE BEBE 900Gx10UN", 30.4m, 3.04m, "ALI-LAS-0130", 100, 10 },
                    { new Guid("00000000-0000-4000-8000-000000000131"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 17.79m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 640, 80, "LAS LLAVES DETERGENTE BEBE 400Gx16UN", 22.24m, 1.39m, "ALI-LAS-0131", 272, 16 },
                    { new Guid("00000000-0000-4000-8000-000000000132"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 13.95m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 640, 80, "LAS LLAVES LINEA ACTIVA CITRICA 400GRX16UN", 17.44m, 1.09m, "ALI-LAS-0132", 384, 16 },
                    { new Guid("00000000-0000-4000-8000-000000000133"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 13.95m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 640, 80, "LAS LLAVES LINEA ACTIVA FLORAL 400GRX16UN", 17.44m, 1.09m, "ALI-LAS-0133", 496, 16 },
                    { new Guid("00000000-0000-4000-8000-000000000134"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 18.64m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 400, 50, "LAS LLAVES LINEA ACTIVA FLORAL 900GRX10UN", 23.3m, 2.33m, "ALI-LAS-0134", 380, 10 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "UnitsPerBox" },
                values: new object[] { new Guid("00000000-0000-4000-8000-000000000135"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 18.64m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 400, 50, "LAS LLAVES LINEA ACTIVA CITRICO 900GRX10UN", 23.3m, 2.33m, "ALI-LAS-0135", 10 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-4000-8000-000000000136"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 69.41m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 1440, 180, "LAS LLAVES MULTIUSO CREMA 250Gx36UN", 86.76m, 2.41m, "ALI-LAS-0136", 252, 36 },
                    { new Guid("00000000-0000-4000-8000-000000000137"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 62.47m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 720, 90, "LAS LLAVES MULTIUSO CREMA 500Gx18UN", 78.09m, 4.34m, "ALI-LAS-0137", 252, 18 },
                    { new Guid("00000000-0000-4000-8000-000000000138"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 58.65m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 640, 80, "LAS LLAVES LAV. L. ANTIBACT 500CCGx16UN", 73.31m, 4.58m, "ALI-LAS-0138", 336, 16 },
                    { new Guid("00000000-0000-4000-8000-000000000139"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 42.54m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "LAS LLAVES LIMP MAREA CRISTALINA 1LX12UN", 53.17m, 4.43m, "ALI-LAS-0139", 336, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000140"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 42.54m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "LAS LLAVES LIMP BRISA TROPICAL 1LX12UN", 53.17m, 4.43m, "ALI-LAS-0140", 420, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000141"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 48.10m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "LAS LLAVES LIMP. MAREA CRISTA 500CCx24UN", 60.13m, 2.51m, "ALI-LAS-0141", 1008, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000142"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 48.10m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "LAS LLAVES LIMP BOSQUE SERENO 500CCx24UN", 60.13m, 2.51m, "ALI-LAS-0142", 96, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000143"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 42.54m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "LAS LLAVES LIMP BOSQUE SERENO 1Lx12UN", 53.17m, 4.43m, "ALI-LAS-0143", 132, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000144"), "Las Llaves", new Guid("a1111111-1111-4111-8111-111111111111"), 42.54m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "LAS LLAVES LIMP FRESCURA R. 1LX12UN", 53.17m, 4.43m, "ALI-LAS-0144", 216, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000145"), "Multi Clean", new Guid("a1111111-1111-4111-8111-111111111111"), 37.30m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 800, 100, "MULTI CLEAN DETER FRAG CITRICA 900Gx20UN", 46.63m, 2.33m, "ALI-MUL-0145", 500, 20 },
                    { new Guid("00000000-0000-4000-8000-000000000146"), "Multi Clean", new Guid("a1111111-1111-4111-8111-111111111111"), 26.17m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 1200, 150, "MULTI CLEAN DETER FRAG CITRICA 400Gx30UN", 32.71m, 1.09m, "ALI-MUL-0146", 960, 30 },
                    { new Guid("00000000-0000-4000-8000-000000000147"), "Multi Clean", new Guid("a1111111-1111-4111-8111-111111111111"), 41.50m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 160, 20, "MULTI CLEAN DETER FRAG CITRICA 5KGx4UN", 51.88m, 12.97m, "ALI-MUL-0147", 156, 4 },
                    { new Guid("00000000-0000-4000-8000-000000000148"), "Multi Clean", new Guid("a1111111-1111-4111-8111-111111111111"), 37.30m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 800, 100, "MULTI CLEAN DETER FRAG FLORAL 900Gx20UN", 46.63m, 2.33m, "ALI-MUL-0148", 20, 20 },
                    { new Guid("00000000-0000-4000-8000-000000000149"), "Multi Clean", new Guid("a1111111-1111-4111-8111-111111111111"), 26.17m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 1200, 150, "MULTI CLEAN DETER FRAG FLORAL 400Gx30UN", 32.71m, 1.09m, "ALI-MUL-0149", 240, 30 },
                    { new Guid("00000000-0000-4000-8000-000000000150"), "Multi Clean", new Guid("a1111111-1111-4111-8111-111111111111"), 41.50m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 160, 20, "MULTI CLEAN DETER FRAG FLORAL 5KGx4UN", 51.88m, 12.97m, "ALI-MUL-0150", 60, 4 },
                    { new Guid("00000000-0000-4000-8000-000000000151"), "Super Can", new Guid("a1111111-1111-4111-8111-111111111111"), 37.53m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "SUPER CAN CACHORRO 2KGx6UN", 46.91m, 7.82m, "ALI-SUP-0151", 132, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000152"), "Super Can", new Guid("a1111111-1111-4111-8111-111111111111"), 38.16m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 40, 5, "SUPER CAN CACHORRO 18KGx1UN", 47.7m, 47.7m, "ALI-SUP-0152", 29, 1 },
                    { new Guid("00000000-0000-4000-8000-000000000153"), "Super Can", new Guid("a1111111-1111-4111-8111-111111111111"), 33.74m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 40, 5, "SUPER CAN CARNE HUESO 18KILOS", 42.18m, 42.18m, "ALI-SUP-0153", 36, 1 },
                    { new Guid("00000000-0000-4000-8000-000000000154"), "Super Can", new Guid("a1111111-1111-4111-8111-111111111111"), 33.74m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 40, 5, "SUPER CAN POLLO 18KILOS", 42.18m, 42.18m, "ALI-SUP-0154", 43, 1 },
                    { new Guid("00000000-0000-4000-8000-000000000155"), "Super Can", new Guid("a1111111-1111-4111-8111-111111111111"), 49.32m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 200, 25, "SUPER CAN CARNE HUESO 4KGx5UN", 61.65m, 12.33m, "ALI-SUP-0155", 25, 5 },
                    { new Guid("00000000-0000-4000-8000-000000000156"), "Super Can", new Guid("a1111111-1111-4111-8111-111111111111"), 55.08m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 200, 25, "SUPER CAN CACHORROS 4KGx5UN", 68.85m, 13.77m, "ALI-SUP-0156", 60, 5 },
                    { new Guid("00000000-0000-4000-8000-000000000157"), "Super Can", new Guid("a1111111-1111-4111-8111-111111111111"), 30.06m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "SUPER CAN CARNE HUESO 2KGx6UN", 37.58m, 6.26m, "ALI-SUP-0157", 114, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000158"), "Super Can", new Guid("a1111111-1111-4111-8111-111111111111"), 22.33m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 40, 5, "SUPER CAN CARNE HUESO 10KGx1UN", 27.91m, 27.91m, "ALI-SUP-0158", 26, 1 },
                    { new Guid("00000000-0000-4000-8000-000000000159"), "Champs", new Guid("a1111111-1111-4111-8111-111111111111"), 27.84m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 40, 5, "CHAMPS PERRO CARNE TACO 20KGx1UN", 34.8m, 34.8m, "ALI-CHA-0159", 33, 1 },
                    { new Guid("00000000-0000-4000-8000-000000000160"), "Dogourmet", new Guid("a1111111-1111-4111-8111-111111111111"), 44.94m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 40, 5, "DOGOURMET CARNE A LA PARRILLA 18KGx1UN", 56.18m, 56.18m, "ALI-DOG-0160", 40, 1 },
                    { new Guid("00000000-0000-4000-8000-000000000161"), "Dogourmet", new Guid("a1111111-1111-4111-8111-111111111111"), 41.42m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "DOGOURMET CARNE A LA PARRILLA 2KGx6UN", 51.78m, 8.63m, "ALI-DOG-0161", 12, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000162"), "Dogourmet", new Guid("a1111111-1111-4111-8111-111111111111"), 44.94m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 40, 5, "DOGOURMET POLLO A LA BRASA 18KGx1UN", 56.18m, 56.18m, "ALI-DOG-0162", 9, 1 },
                    { new Guid("00000000-0000-4000-8000-000000000163"), "Dogourmet", new Guid("a1111111-1111-4111-8111-111111111111"), 50.60m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 40, 5, "DOGOURMET CACHORROS 18KGx1UN", 63.25m, 63.25m, "ALI-DOG-0163", 16, 1 },
                    { new Guid("00000000-0000-4000-8000-000000000164"), "Dogourmet", new Guid("a1111111-1111-4111-8111-111111111111"), 45.38m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "DOGOURMET CACHORROS 2KGx6UN", 56.72m, 9.45m, "ALI-DOG-0164", 138, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000165"), "Dogourmet", new Guid("a1111111-1111-4111-8111-111111111111"), 64.73m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 200, 25, "DOGOURMET CARNE A LA PARRILLA 4KGx5UN", 80.91m, 16.18m, "ALI-DOG-0165", 150, 5 },
                    { new Guid("00000000-0000-4000-8000-000000000166"), "Dogourmet", new Guid("a1111111-1111-4111-8111-111111111111"), 64.73m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 200, 25, "DOGOURMET POLLO A LA BRASA 4KGx5UN", 80.91m, 16.18m, "ALI-DOG-0166", 185, 5 },
                    { new Guid("00000000-0000-4000-8000-000000000167"), "Dogourmet", new Guid("a1111111-1111-4111-8111-111111111111"), 36.29m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 400, 50, "DOGOURMET CARNE A LA PARRILLA 1KGx10UN", 45.36m, 4.54m, "ALI-DOG-0167", 440, 10 },
                    { new Guid("00000000-0000-4000-8000-000000000168"), "Dogourmet", new Guid("a1111111-1111-4111-8111-111111111111"), 44.94m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 40, 5, "DOGOURMET ASADO NEGRO 18KGx1UN", 56.18m, 56.18m, "ALI-DOG-0168", 6, 1 },
                    { new Guid("00000000-0000-4000-8000-000000000169"), "Dogourmet", new Guid("a1111111-1111-4111-8111-111111111111"), 64.73m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 200, 25, "DOGOURMET ASADO NEGRO 4KGx5UN", 80.91m, 16.18m, "ALI-DOG-0169", 65, 5 },
                    { new Guid("00000000-0000-4000-8000-000000000170"), "Dogourmet", new Guid("a1111111-1111-4111-8111-111111111111"), 47.25m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 40, 5, "DOGOURMET PARRILLA MIXTA 18KGx1UN", 59.06m, 59.06m, "ALI-DOG-0170", 20, 1 },
                    { new Guid("00000000-0000-4000-8000-000000000171"), "Dogourmet", new Guid("a1111111-1111-4111-8111-111111111111"), 67.88m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 200, 25, "DOGOURMET PARRILLA MIXTA 4KGx5UN", 84.85m, 16.97m, "ALI-DOG-0171", 135, 5 },
                    { new Guid("00000000-0000-4000-8000-000000000172"), "Dogourmet", new Guid("a1111111-1111-4111-8111-111111111111"), 30.93m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 40, 5, "DOGOURMET CACHORROS 10KGx1UN", 38.66m, 38.66m, "ALI-DOG-0172", 34, 1 },
                    { new Guid("00000000-0000-4000-8000-000000000173"), "Dogourmet", new Guid("a1111111-1111-4111-8111-111111111111"), 28.14m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 40, 5, "DOGOURMET CARNE A LA PARRILLA 10KGx1UN", 35.17m, 35.17m, "ALI-DOG-0173", 41, 1 },
                    { new Guid("00000000-0000-4000-8000-000000000174"), "Ohmaigat", new Guid("a1111111-1111-4111-8111-111111111111"), 62.42m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "OHMAIGAT CASEROS Y DELICADOS 1,5KGX6UND", 78.02m, 13m, "ALI-OHM-0174", 18, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000175"), "Ohmaigat", new Guid("a1111111-1111-4111-8111-111111111111"), 43.66m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "OHMAIGAT CASEROS Y DELICADOS 500Gx12UND", 54.57m, 4.55m, "ALI-OHM-0175", 120, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000176"), "Donkat", new Guid("a1111111-1111-4111-8111-111111111111"), 89.54m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 720, 90, "DONKAT GATICOS 1KGX18UND", 111.92m, 6.22m, "ALI-DON-0176", 306, 18 },
                    { new Guid("00000000-0000-4000-8000-000000000177"), "Donkat", new Guid("a1111111-1111-4111-8111-111111111111"), 78.09m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 600, 75, "DONKAT ADULTO 1,1KGX15UND", 97.61m, 6.51m, "ALI-DON-0177", 360, 15 },
                    { new Guid("00000000-0000-4000-8000-000000000178"), "Donkat", new Guid("a1111111-1111-4111-8111-111111111111"), 22.46m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 40, 5, "DONKAT ADULTO 7KGX1UND", 28.08m, 28.08m, "ALI-DON-0178", 31, 1 },
                    { new Guid("00000000-0000-4000-8000-000000000179"), "Donkat", new Guid("a1111111-1111-4111-8111-111111111111"), 54.18m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 40, 5, "DONKAT ADULTO 16KGx1UND", 67.72m, 67.72m, "ALI-DON-0179", 38, 1 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "UnitsPerBox" },
                values: new object[] { new Guid("00000000-0000-4000-8000-000000000180"), "Donkat", new Guid("a1111111-1111-4111-8111-111111111111"), 23.35m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 40, 5, "DONKAT GATICOS 7KGX1UND", 29.19m, 29.19m, "ALI-DON-0180", 1 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-4000-8000-000000000181"), "Gillette", new Guid("c3333333-3333-4333-8333-333333333333"), 93.32m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "GILLETTE BOLSA PB2X5UN MASC 39GX24UN", 116.65m, 4.86m, "JAB-GIL-0181", 168, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000182"), "Gillette", new Guid("c3333333-3333-4333-8333-333333333333"), 448.56m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "GILLETTE RISTRA PB2X24UN MASC 168GX24RIS", 560.7m, 23.36m, "JAB-GIL-0182", 336, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000183"), "Gillette", new Guid("c3333333-3333-4333-8333-333333333333"), 12.46m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 400, 50, "GILLETTE RISTRA PB3X10UN MASC 120GX12RIS", 15.57m, 1.55m, "JAB-GIL-0183", 210, 10 },
                    { new Guid("00000000-0000-4000-8000-000000000184"), "Gillette", new Guid("c3333333-3333-4333-8333-333333333333"), 227.84m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 2880, 360, "GILLETTE BLIST PB3X2 SENSECARE 23GX72UN", 284.8m, 3.96m, "JAB-GIL-0184", 2016, 72 },
                    { new Guid("00000000-0000-4000-8000-000000000185"), "Gillette", new Guid("c3333333-3333-4333-8333-333333333333"), 229.18m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 2880, 360, "GILLETTE BLISTER CUERPOX2 23GX72UND", 286.47m, 3.98m, "JAB-GIL-0185", 2520, 72 },
                    { new Guid("00000000-0000-4000-8000-000000000186"), "Venus", new Guid("c3333333-3333-4333-8333-333333333333"), 99.58m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 400, 50, "VENUS RISTRA SIMPLYX8UN FEM 104GX10RIS", 124.47m, 12.45m, "JAB-VEN-0186", 420, 10 },
                    { new Guid("00000000-0000-4000-8000-000000000187"), "Oralb", new Guid("c3333333-3333-4333-8333-333333333333"), 65.76m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "ORALB CREMADENT 100% 6X50ML X4UN", 82.2m, 13.7m, "JAB-ORA-0187", 24, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000188"), "Oralb", new Guid("c3333333-3333-4333-8333-333333333333"), 26.00m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "ORALB CEP 1-2-3 PAQX6UN 120GX6RIS", 32.5m, 5.42m, "JAB-ORA-0188", 66, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000189"), "Ace", new Guid("c3333333-3333-4333-8333-333333333333"), 36.33m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 600, 75, "ACE DETERGENTE POLVO COL 800GX15UN", 45.41m, 3.03m, "JAB-ACE-0189", 270, 15 },
                    { new Guid("00000000-0000-4000-8000-000000000190"), "Ace", new Guid("c3333333-3333-4333-8333-333333333333"), 30.06m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "ACE DETERGENTE POLVO COL 400GX24UN", 37.58m, 1.57m, "JAB-ACE-0190", 600, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000191"), "Ariel", new Guid("c3333333-3333-4333-8333-333333333333"), 43.98m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 600, 75, "ARIEL DETER TOUCH OF DOWNY COL 800GX15UN", 54.98m, 3.67m, "JAB-ARI-0191", 480, 15 },
                    { new Guid("00000000-0000-4000-8000-000000000192"), "Ariel", new Guid("c3333333-3333-4333-8333-333333333333"), 51.56m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 160, 20, "ARIEL DETERGENTE POLVO COL 4KGX4UN", 64.45m, 16.11m, "JAB-ARI-0192", 156, 4 },
                    { new Guid("00000000-0000-4000-8000-000000000193"), "Ariel", new Guid("c3333333-3333-4333-8333-333333333333"), 43.98m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 600, 75, "ARIEL DETERGENTE POLVO COL 800GX15UN", 54.98m, 3.67m, "JAB-ARI-0193", 15, 15 },
                    { new Guid("00000000-0000-4000-8000-000000000194"), "Ariel", new Guid("c3333333-3333-4333-8333-333333333333"), 36.08m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "ARIEL DETERGENTE POLVO COL 400GX24UN", 45.1m, 1.88m, "JAB-ARI-0194", 192, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000195"), "Downy", new Guid("c3333333-3333-4333-8333-333333333333"), 56.02m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 360, 45, "DOWNY SUAVIZANTE FLORAL 1400CCX9UN", 70.02m, 7.78m, "JAB-DOW-0195", 135, 9 },
                    { new Guid("00000000-0000-4000-8000-000000000196"), "Downy", new Guid("c3333333-3333-4333-8333-333333333333"), 39.46m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "DOWNY SUAVIZANTE AMANECER 700CCX12UN", 49.32m, 4.11m, "JAB-DOW-0196", 264, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000197"), "Downy", new Guid("c3333333-3333-4333-8333-333333333333"), 39.46m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "DOWNY SUAVIZANTE FLORAL 700CCX12UN", 49.32m, 4.11m, "JAB-DOW-0197", 348, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000198"), "Downy", new Guid("c3333333-3333-4333-8333-333333333333"), 24.86m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "DOWNY SUAVIZANTE FLORAL 360CCX12UN", 31.08m, 2.59m, "JAB-DOW-0198", 432, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000199"), "Downy", new Guid("c3333333-3333-4333-8333-333333333333"), 24.86m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "DOWNY SUAVIZANTE AMANECER 360CCX12UN", 31.08m, 2.59m, "JAB-DOW-0199", 516, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000200"), "Downy", new Guid("c3333333-3333-4333-8333-333333333333"), 39.46m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "DOWNY SUAVIZANTE EXOTICO 700CCX12UN", 49.32m, 4.11m, "JAB-DOW-0200", 60, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000201"), "Downy", new Guid("c3333333-3333-4333-8333-333333333333"), 53.37m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 360, 45, "DOWNY SUAVIZANTE EXOTICO 1400CCX9UN", 66.71m, 7.41m, "JAB-DOW-0201", 108, 9 },
                    { new Guid("00000000-0000-4000-8000-000000000202"), "Downy", new Guid("c3333333-3333-4333-8333-333333333333"), 23.61m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "DOWNY SUAVIZANTE EXOTICO 360CCX12UN", 29.51m, 2.46m, "JAB-DOW-0202", 228, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000203"), "Pampers", new Guid("c3333333-3333-4333-8333-333333333333"), 38.98m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 320, 40, "PAMPERS PAÑALES CONFORT SEC XXG 432GX8UN", 48.72m, 6.09m, "JAB-PAM-0203", 208, 8 },
                    { new Guid("00000000-0000-4000-8000-000000000204"), "Pampers", new Guid("c3333333-3333-4333-8333-333333333333"), 38.98m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 320, 40, "PAMPERS PAÑALES CONFORT SEC XG 451GX8UN", 48.72m, 6.09m, "JAB-PAM-0204", 264, 8 },
                    { new Guid("00000000-0000-4000-8000-000000000205"), "Pampers", new Guid("c3333333-3333-4333-8333-333333333333"), 38.98m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 320, 40, "PAMPERS PAÑALES CONFORT SEC G 463GX8UN", 48.72m, 6.09m, "JAB-PAM-0205", 320, 8 },
                    { new Guid("00000000-0000-4000-8000-000000000206"), "Pampers", new Guid("c3333333-3333-4333-8333-333333333333"), 58.46m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PAMPERS PAÑALES CONFORT SEC M 491GX12UN", 73.08m, 6.09m, "JAB-PAM-0206", 24, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000207"), "Pampers", new Guid("c3333333-3333-4333-8333-333333333333"), 19.49m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 160, 20, "PAMPERS PAÑALES CONFORT SEC P 432GX4UN", 24.36m, 6.09m, "JAB-PAM-0207", 36, 4 },
                    { new Guid("00000000-0000-4000-8000-000000000208"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 78.51m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN ACOND RESTAURACION 400CCX12UN", 98.14m, 8.18m, "JAB-PTN-0208", 192, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000209"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 47.22m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN ACOND RESTAURACION 200CCX12UN", 59.02m, 4.92m, "JAB-PTN-0209", 276, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000210"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 86.08m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN ACOND 3MM RESTAURACION 170CCX12UN", 107.6m, 8.97m, "JAB-PTN-0210", 360, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000211"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 86.08m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN ACOND 3MM LISO EXT 170CCX12UN", 107.6m, 8.97m, "JAB-PTN-0211", 444, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000212"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 86.08m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN ACOND 3MM HIDRAT EXTR 170CCX12UN", 107.6m, 8.97m, "JAB-PTN-0212", 528, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000213"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 47.22m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN SHAMPOO BAMBU 200CCX12UN", 59.02m, 4.92m, "JAB-PTN-0213", 72, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000214"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 78.51m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN SHAMPOO BAMBU 400CCX12UN", 98.14m, 8.18m, "JAB-PTN-0214", 156, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000215"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 78.51m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN ACOND BAMBU 400CCX12UN", 98.14m, 8.18m, "JAB-PTN-0215", 240, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000216"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 47.22m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN ACOND BAMBU 200CCX12UN", 59.02m, 4.92m, "JAB-PTN-0216", 324, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000217"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 87.19m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN INTENSIVE TREATMENT 300CCX12UN", 108.99m, 9.08m, "JAB-PTN-0217", 408, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000218"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 87.19m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN TRATAMIENTO HIDRATACIÓN 300CCX12UN", 108.99m, 9.08m, "JAB-PTN-0218", 492, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000219"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 87.19m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN TRATAMIENTO NUTRICIÓN 300CCX12UN", 108.99m, 9.08m, "JAB-PTN-0219", 36, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000220"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 78.51m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN SHAMPOO COLAGENO 300CCX12UN", 98.14m, 8.18m, "JAB-PTN-0220", 120, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000221"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 125.84m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN SHAMPOO COLAGENO 510CCX12UN", 157.3m, 13.11m, "JAB-PTN-0221", 204, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000222"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 125.84m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN ACOND COLAGENO 510CCX12UN", 157.3m, 13.11m, "JAB-PTN-0222", 288, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000223"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 75.06m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN CREMA PEINAR COLAGENO 300CCX12UN", 93.82m, 7.82m, "JAB-PTN-0223", 372, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000224"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 78.51m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN ACOND COLAGENO 250CCX12UN", 98.14m, 8.18m, "JAB-PTN-0224", 456, 12 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "UnitsPerBox" },
                values: new object[] { new Guid("00000000-0000-4000-8000-000000000225"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 75.06m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN CREMA PEINAR RESTAURACION 300CCX12UN", 93.82m, 7.82m, "JAB-PTN-0225", 12 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-4000-8000-000000000226"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 75.06m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN CREMA PEINAR LISO EXT 300CCX12UN", 93.82m, 7.82m, "JAB-PTN-0226", 84, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000227"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 47.23m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN SHAMPOO RESTAURACION 200CCX12UN", 59.04m, 4.92m, "JAB-PTN-0227", 168, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000228"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 78.51m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN SHAMPOO RESTAURACION 400CCX12UN", 98.14m, 8.18m, "JAB-PTN-0228", 252, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000229"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 125.84m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN ACOND KERATINA 510CCX12UN", 157.3m, 13.11m, "JAB-PTN-0229", 336, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000230"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 125.84m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN SHAMPOO KERATINA 510CCX12UN", 157.3m, 13.11m, "JAB-PTN-0230", 420, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000231"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 75.06m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN CREMA PEINAR KERATINA 300CCX12UN", 93.82m, 7.82m, "JAB-PTN-0231", 504, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000232"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 87.19m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN ACOND KERATINA 250CCX12UN", 108.99m, 9.08m, "JAB-PTN-0232", 48, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000233"), "Ptn", new Guid("c3333333-3333-4333-8333-333333333333"), 87.19m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN SHAMPOO KERATINA 300CCX12UN", 108.99m, 9.08m, "JAB-PTN-0233", 132, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000234"), "Hs", new Guid("c3333333-3333-4333-8333-333333333333"), 82.18m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "HS ACOND ANTIFALL 300CCX12UN", 102.73m, 8.56m, "JAB-HSA-0234", 216, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000235"), "Hs", new Guid("c3333333-3333-4333-8333-333333333333"), 82.18m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "HS SHAMPOO 2en1 LIMP RENOV 375CCX12UN", 102.73m, 8.56m, "JAB-HSS-0235", 300, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000236"), "Hs", new Guid("c3333333-3333-4333-8333-333333333333"), 82.18m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "HS SHAMPOO ANTIFALL 375CCX12UN", 102.73m, 8.56m, "JAB-HSS-0236", 384, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000237"), "Hs", new Guid("c3333333-3333-4333-8333-333333333333"), 49.55m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "HS SHAMPOO ANTIFALL 180CCX12UN", 61.94m, 5.16m, "JAB-HSS-0237", 468, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000238"), "Hs", new Guid("c3333333-3333-4333-8333-333333333333"), 49.55m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "HS SHAMPOO LIMP RENOV 180CCX12UN", 61.94m, 5.16m, "JAB-HSS-0238", 12, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000239"), "Hs", new Guid("c3333333-3333-4333-8333-333333333333"), 82.18m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "HS SHAMPOO LIMP RENOV 375CCX12UN", 102.73m, 8.56m, "JAB-HSS-0239", 96, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000240"), "Hs", new Guid("c3333333-3333-4333-8333-333333333333"), 82.18m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "HS SHAMPOO C/OLD SPICE 375MLX12UN", 102.73m, 8.56m, "JAB-HSS-0240", 180, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000241"), "Hs", new Guid("c3333333-3333-4333-8333-333333333333"), 49.55m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "HS SHAMPOO 2en1 SUAVE Y MANEJ 180CCX12UN", 61.94m, 5.16m, "JAB-HSS-0241", 264, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000242"), "Hs", new Guid("c3333333-3333-4333-8333-333333333333"), 82.18m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "HS SHAMPOO 2en1 SUAVE Y MANEJ 375CCX12UN", 102.73m, 8.56m, "JAB-HSS-0242", 348, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000243"), "Hs", new Guid("c3333333-3333-4333-8333-333333333333"), 82.18m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "HS SHAMPOO ACEITE DE COCO 375CCX12UN", 102.73m, 8.56m, "JAB-HSS-0243", 432, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000244"), "Hs", new Guid("c3333333-3333-4333-8333-333333333333"), 82.18m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "HS SHAMPOO ANTI COMEZON 375CCX12UN", 102.73m, 8.56m, "JAB-HSS-0244", 516, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000245"), "Hs", new Guid("c3333333-3333-4333-8333-333333333333"), 49.55m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "HS SHAMPOO ANTI COMEZON 180CCX12UN", 61.94m, 5.16m, "JAB-HSS-0245", 60, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000246"), "Always", new Guid("c3333333-3333-4333-8333-333333333333"), 78.51m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "ALWAYS INFINITY NOCT 16 UND 89GX12UN", 98.14m, 8.18m, "JAB-ALW-0246", 144, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000247"), "Always", new Guid("c3333333-3333-4333-8333-333333333333"), 78.51m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "ALWAYS INFINITY DIA 18 UND 89GX12UN", 98.14m, 8.18m, "JAB-ALW-0247", 228, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000248"), "Always", new Guid("c3333333-3333-4333-8333-333333333333"), 77.28m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "ALWAYS PROT DIARIOS S/PERF 40UN 90GX24UN", 96.6m, 4.03m, "JAB-ALW-0248", 624, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000249"), "Always", new Guid("c3333333-3333-4333-8333-333333333333"), 46.55m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "ALWAYS TOALLAS ULT SUAVE NOCHE 43GX24UN", 58.19m, 2.42m, "JAB-ALW-0249", 792, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000250"), "Always", new Guid("c3333333-3333-4333-8333-333333333333"), 45.66m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "ALWAYS TOALLAS ULTRA SECA DIA 37GX24UN", 57.07m, 2.38m, "JAB-ALW-0250", 960, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000251"), "Always", new Guid("c3333333-3333-4333-8333-333333333333"), 58.58m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "ALWAYS TOALLAS ULT SUAVE DIA 45GX24UN", 73.22m, 3.05m, "JAB-ALW-0251", 48, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000252"), "Always", new Guid("c3333333-3333-4333-8333-333333333333"), 50.34m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "ALWAYS TOALLAS ULTRA SECA NOCHE 45GX24UN", 62.92m, 2.62m, "JAB-ALW-0252", 216, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000253"), "Always", new Guid("c3333333-3333-4333-8333-333333333333"), 44.21m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "ALWAYS TOALLAS ULT SUAVE NOCHE 87GX12UN", 55.26m, 4.61m, "JAB-ALW-0253", 192, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000254"), "Always", new Guid("c3333333-3333-4333-8333-333333333333"), 25.94m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "ALWAYS TOALL NOCT SEDA F ABUND 101GX12UN", 32.43m, 2.7m, "JAB-ALW-0254", 276, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000255"), "Always", new Guid("c3333333-3333-4333-8333-333333333333"), 41.65m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "ALWAYS TOALLAS SEDASEC F ABUND 60GX24UN", 52.06m, 2.17m, "JAB-ALW-0255", 720, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000256"), "Always", new Guid("c3333333-3333-4333-8333-333333333333"), 63.58m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "ALWAYS TOALL NOCT SEDA F ABUND 277GX12UN", 79.48m, 6.62m, "JAB-ALW-0256", 444, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000257"), "Gillette", new Guid("c3333333-3333-4333-8333-333333333333"), 27.62m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "GILLETTE ANTIT ROLL-ON WAVE 60GX12UN", 34.52m, 2.88m, "JAB-GIL-0257", 528, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000258"), "Gillette", new Guid("c3333333-3333-4333-8333-333333333333"), 24.83m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "GILLETTE ANTIT ROLL-ON RUSH 60GX12UN", 31.04m, 2.59m, "JAB-GIL-0258", 72, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000259"), "Secret", new Guid("c3333333-3333-4333-8333-333333333333"), 26.73m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "SECRET ANTIT ROLL-ON PROTECT 60GX12UN", 33.41m, 2.78m, "JAB-SEC-0259", 156, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000260"), "MINALBA", new Guid("b2222222-2222-4222-8222-222222222222"), 10.96m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "MINALBA SPARKLYN LIMON 500MLX12UN", 13.7m, 1.14m, "BEB-MIN-0260", 240, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000261"), "MAZEITE", new Guid("a1111111-1111-4111-8111-111111111111"), 46.08m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "MAZEITE 1L X 12UND", 57.6m, 4.8m, "ALI-MAZ-0261", 324, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000262"), "RIKESA", new Guid("a1111111-1111-4111-8111-111111111111"), 78.67m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 720, 90, "RIKESA QUESO TOCINETA 330Gx18UN", 98.34m, 5.46m, "ALI-RIK-0262", 612, 18 },
                    { new Guid("00000000-0000-4000-8000-000000000263"), "MINALBA", new Guid("b2222222-2222-4222-8222-222222222222"), 20.80m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "MINALBA SPARKLING GINGERBEER LAT355ML X24", 26m, 1.08m, "BEB-MIN-0263", 984, 24 },
                    { new Guid("00000000-0000-4000-8000-000000000264"), "LAS LLAVES", new Guid("a1111111-1111-4111-8111-111111111111"), 39.33m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 160, 20, "LAS LLAVES LINEA ACTIVA 5KG x 4und", 49.16m, 12.29m, "ALI-LAS-0264", 12, 4 },
                    { new Guid("00000000-0000-4000-8000-000000000265"), "ORAL B", new Guid("c3333333-3333-4333-8333-333333333333"), 5.21m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 160, 20, "CEPILLO ORAL B INDICAOR X4 RISTRA", 6.51m, 1.62m, "JAB-CEP-0265", 40, 4 },
                    { new Guid("00000000-0000-4000-8000-000000000266"), "PEPSI", new Guid("b2222222-2222-4222-8222-222222222222"), 4.40m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "PEPSI/SABORES 1,5LX6UN", 5.5m, 0.92m, "BEB-PEP-0266", 102, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000267"), "CARORENA", new Guid("b2222222-2222-4222-8222-222222222222"), 30.90m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "SANGRIA CAROREÑA TORONJA 1,75Lx6UN", 38.63m, 6.43m, "BEB-SAN-0267", 144, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000268"), "HS", new Guid("c3333333-3333-4333-8333-333333333333"), 49.55m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "HS ANTIRESEQUEDAD 180MLX12", 61.94m, 5.16m, "JAB-HSA-0268", 372, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000269"), "MINALBA", new Guid("b2222222-2222-4222-8222-222222222222"), 20.80m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 960, 120, "MINALBA AGUAKINA LATA 355X24UND", 26m, 1.08m, "BEB-MIN-0269", 912, 24 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "UnitsPerBox" },
                values: new object[] { new Guid("00000000-0000-4000-8000-000000000270"), "PEPSI", new Guid("b2222222-2222-4222-8222-222222222222"), 6.40m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "PEPSI 2L X 6UND", 8.0m, 1.33m, "BEB-PEP-0270", 6 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-4000-8000-000000000271"), "PEPSI", new Guid("b2222222-2222-4222-8222-222222222222"), 6.40m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "PEPSI ZERO 2L X6UND", 8m, 1.33m, "BEB-PEP-0271", 42, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000272"), "PEPSI", new Guid("b2222222-2222-4222-8222-222222222222"), 3.61m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 240, 30, "PEPSI ZERO 1 L X 6UND", 4.51m, 0.75m, "BEB-PEP-0272", 84, 6 },
                    { new Guid("00000000-0000-4000-8000-000000000273"), "LLAVES", new Guid("a1111111-1111-4111-8111-111111111111"), 17.72m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 400, 50, "LLAVES DETG CARICIAS CITRICAS LINEA ACTIVA 900GRX10UN", 22.15m, 2.21m, "ALI-LLA-0273", 210, 10 },
                    { new Guid("00000000-0000-4000-8000-000000000274"), "PANTENE", new Guid("c3333333-3333-4333-8333-333333333333"), 45.25m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "PTN KERATINA 175GRS", 56.56m, 4.88m, "JAB-PTN-0274", 336, 12 },
                    { new Guid("00000000-0000-4000-8000-000000000275"), "GILLETTE", new Guid("c3333333-3333-4333-8333-333333333333"), 33.02m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, null, 480, 60, "GILLETTE GEL 42GRS", 41.28m, 3.44m, "JAB-GIL-0275", 420, 12 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
