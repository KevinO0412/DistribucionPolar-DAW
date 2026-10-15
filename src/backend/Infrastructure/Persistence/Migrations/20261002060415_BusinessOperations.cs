using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BusinessOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttendantName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanChangeAttendant",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGiftEligible",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Products",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BeforeJson = table.Column<string>(type: "jsonb", nullable: true),
                    AfterJson = table.Column<string>(type: "jsonb", nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IpAddress = table.Column<IPAddress>(type: "inet", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Rif = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Nickname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MoneyBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    EmptyBoxesBalance = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EmptyUnitsBalance = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsignmentEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Responsible = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventDate = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsignmentEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentCounters",
                columns: table => new
                {
                    Sequence = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    CurrentNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentCounters", x => new { x.Sequence, x.Year, x.Month });
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trucks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trucks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientEmptyBalances",
                columns: table => new
                {
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Boxes = table.Column<int>(type: "integer", nullable: false),
                    Units = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientEmptyBalances", x => new { x.ClientId, x.GroupKey });
                    table.ForeignKey(
                        name: "FK_ClientEmptyBalances_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsignmentItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PriceBox = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceUnit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitsPerBox = table.Column<int>(type: "integer", nullable: false),
                    DeliveredUnits = table.Column<int>(type: "integer", nullable: false),
                    ReturnedUnits = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsignmentItems", x => x.Id);
                    table.CheckConstraint("CK_ConsignmentItems_Delivered_Positive", "\"DeliveredUnits\" > 0");
                    table.CheckConstraint("CK_ConsignmentItems_Returned_Valid", "\"ReturnedUnits\" >= 0 AND \"ReturnedUnits\" <= \"DeliveredUnits\"");
                    table.ForeignKey(
                        name: "FK_ConsignmentItems_ConsignmentEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "ConsignmentEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsignmentItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TotalUnits = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purchases_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DamagedProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ActionTaken = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    Origin = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: true),
                    TruckName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Boxes = table.Column<int>(type: "integer", nullable: false),
                    LooseUnits = table.Column<int>(type: "integer", nullable: false),
                    TotalUnits = table.Column<int>(type: "integer", nullable: false),
                    EstimatedValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Observation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamagedProducts", x => x.Id);
                    table.CheckConstraint("CK_DamagedProducts_TotalUnits_Positive", "\"TotalUnits\" > 0");
                    table.ForeignKey(
                        name: "FK_DamagedProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DamagedProducts_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InternalConsumptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    Origin = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: true),
                    TruckName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Boxes = table.Column<int>(type: "integer", nullable: false),
                    LooseUnits = table.Column<int>(type: "integer", nullable: false),
                    TotalUnits = table.Column<int>(type: "integer", nullable: false),
                    EstimatedValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Observation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalConsumptions", x => x.Id);
                    table.CheckConstraint("CK_InternalConsumptions_TotalUnits_Positive", "\"TotalUnits\" > 0");
                    table.ForeignKey(
                        name: "FK_InternalConsumptions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternalConsumptions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternalConsumptions_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ClientRif = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    DispatchOrigin = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: true),
                    TruckName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TruckPlate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    AttendantName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Payment = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Pending = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentObservation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    GeneratedEmptyBoxes = table.Column<int>(type: "integer", nullable: false),
                    GeneratedEmptyUnits = table.Column<int>(type: "integer", nullable: false),
                    ReturnedEmptyBoxes = table.Column<int>(type: "integer", nullable: false),
                    ReturnedEmptyUnits = table.Column<int>(type: "integer", nullable: false),
                    IsCancelled = table.Column<bool>(type: "boolean", nullable: false),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    CancelledBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.CheckConstraint("CK_Invoices_Gift_NoCharge", "\"Type\" <> 'Gift' OR (\"Total\" = 0 AND \"Payment\" = 0 AND \"Pending\" = 0)");
                    table.CheckConstraint("CK_Invoices_Payment_NotNegative", "\"Payment\" >= 0");
                    table.CheckConstraint("CK_Invoices_Total_NotNegative", "\"Total\" >= 0");
                    table.CheckConstraint("CK_Invoices_Truck_Required", "\"DispatchOrigin\" <> 'Truck' OR \"TruckId\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Invoices_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TruckLoads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TruckPlate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    UnitsPerBox = table.Column<int>(type: "integer", nullable: false),
                    TotalUnits = table.Column<int>(type: "integer", nullable: false),
                    StockBefore = table.Column<int>(type: "integer", nullable: false),
                    StockAfter = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceTruckId = table.Column<Guid>(type: "uuid", nullable: true),
                    Observation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckLoads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TruckLoads_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TruckLoads_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TruckStock",
                columns: table => new
                {
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockUnits = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckStock", x => new { x.TruckId, x.ProductId });
                    table.CheckConstraint("CK_TruckStock_StockUnits_Positive", "\"StockUnits\" > 0");
                    table.ForeignKey(
                        name: "FK_TruckStock_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TruckStock_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PriceBox = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceUnit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitsPerBox = table.Column<int>(type: "integer", nullable: false),
                    Boxes = table.Column<int>(type: "integer", nullable: false),
                    LooseUnits = table.Column<int>(type: "integer", nullable: false),
                    TotalUnits = table.Column<int>(type: "integer", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseItems", x => x.Id);
                    table.CheckConstraint("CK_PurchaseItems_TotalUnits_Positive", "\"TotalUnits\" > 0");
                    table.ForeignKey(
                        name: "FK_PurchaseItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseItems_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientMovements_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientMovements_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceEmptyGroups",
                columns: table => new
                {
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GeneratedBoxes = table.Column<int>(type: "integer", nullable: false),
                    GeneratedUnits = table.Column<int>(type: "integer", nullable: false),
                    ReturnedBoxes = table.Column<int>(type: "integer", nullable: false),
                    ReturnedUnits = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceEmptyGroups", x => new { x.InvoiceId, x.GroupKey });
                    table.ForeignKey(
                        name: "FK_InvoiceEmptyGroups_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SKU = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PriceBox = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceUnit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitsPerBox = table.Column<int>(type: "integer", nullable: false),
                    EmptyGroupKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Boxes = table.Column<int>(type: "integer", nullable: false),
                    LooseUnits = table.Column<int>(type: "integer", nullable: false),
                    TotalUnits = table.Column<int>(type: "integer", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.CheckConstraint("CK_InvoiceItems_TotalUnits_Positive", "\"TotalUnits\" > 0");
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "Id", "Address", "Code", "CreatedAt", "LastModifiedAt", "Name", "Nickname", "Rif", "Type" },
                values: new object[,]
                {
                    { new Guid("00000300-0000-4000-8000-000000000001"), "Av. Principal, local 3", 1, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Bodega La Esquina", "La Esquina", "J-40000001-0", "Ocasional" },
                    { new Guid("00000300-0000-4000-8000-000000000002"), "Calle 5 con carrera 7", 2, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Abasto El Progreso", "El Progreso", "J-40000002-0", "ESPECIAL" },
                    { new Guid("00000300-0000-4000-8000-000000000003"), "Av. Los Andes, local 12", 3, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Licorería Los Andes", "Los Andes", "J-40000003-0", "ESPECIAL" },
                    { new Guid("00000300-0000-4000-8000-000000000004"), "Barrio Santa Rosa, calle 2", 4, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Panadería Santa Rosa", "Santa Rosa", "V-10000004", "Ocasional" },
                    { new Guid("00000300-0000-4000-8000-000000000005"), "Carrera 10, centro", 5, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Restaurante El Fogón", "El Fogón", "J-40000005-0", "Ocasional" }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "CreatedAt", "IsActive", "LastModifiedAt", "Name", "Role" },
                values: new object[,]
                {
                    { new Guid("00000100-0000-4000-8000-000000000001"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "Carlos Pérez", "Vendedor" },
                    { new Guid("00000100-0000-4000-8000-000000000002"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "María González", "Vendedora" },
                    { new Guid("00000100-0000-4000-8000-000000000003"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "Luis Hernández", "Vendedor" },
                    { new Guid("00000100-0000-4000-8000-000000000004"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "José Rodríguez", "Despachador" },
                    { new Guid("00000100-0000-4000-8000-000000000005"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "Pedro Ramírez", "Chofer" },
                    { new Guid("00000100-0000-4000-8000-000000000006"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "Ana Martínez", "Administración" },
                    { new Guid("00000100-0000-4000-8000-000000000007"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "Laura Torres", "Vendedora" }
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-4000-8000-000000000001"),
                column: "IsGiftEligible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-4000-8000-000000000004"),
                column: "IsGiftEligible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-4000-8000-000000000006"),
                column: "IsGiftEligible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-4000-8000-000000000012"),
                column: "IsGiftEligible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-4000-8000-000000000021"),
                column: "IsGiftEligible",
                value: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-4000-8000-000000000022"),
                column: "IsGiftEligible",
                value: true);

            migrationBuilder.InsertData(
                table: "Trucks",
                columns: new[] { "Id", "CreatedAt", "LastModifiedAt", "Name", "Plate", "Status" },
                values: new object[,]
                {
                    { new Guid("00000200-0000-4000-8000-000000000001"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cargo 815", "AA100AA", "En galpón" },
                    { new Guid("00000200-0000-4000-8000-000000000002"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "FVR", "AA200AA", "En galpón" },
                    { new Guid("00000200-0000-4000-8000-000000000003"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Mack", "AA300AA", "En galpón" },
                    { new Guid("00000200-0000-4000-8000-000000000004"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Kodiak", "AA400AA", "En galpón" },
                    { new Guid("00000200-0000-4000-8000-000000000005"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "F-350", "AA500AA", "En galpón" },
                    { new Guid("00000200-0000-4000-8000-000000000006"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Montana", "AA600AA", "En galpón" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Module_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "Module", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientMovements_ClientId_CreatedAt",
                table: "ClientMovements",
                columns: new[] { "ClientId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientMovements_InvoiceId",
                table: "ClientMovements",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientMovements_Type_CreatedAt",
                table: "ClientMovements",
                columns: new[] { "Type", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Code",
                table: "Clients",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Name",
                table: "Clients",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Rif",
                table: "Clients",
                column: "Rif",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignmentEvents_IsClosed",
                table: "ConsignmentEvents",
                column: "IsClosed");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignmentItems_EventId",
                table: "ConsignmentItems",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignmentItems_ProductId",
                table: "ConsignmentItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DamagedProducts_OccurredAt",
                table: "DamagedProducts",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_DamagedProducts_ProductId",
                table: "DamagedProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DamagedProducts_TruckId",
                table: "DamagedProducts",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Name",
                table: "Employees",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_InternalConsumptions_EmployeeId",
                table: "InternalConsumptions",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalConsumptions_OccurredAt",
                table: "InternalConsumptions",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_InternalConsumptions_ProductId",
                table: "InternalConsumptions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalConsumptions_TruckId",
                table: "InternalConsumptions",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_ProductId",
                table: "InvoiceItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ClientId_IssuedAt",
                table: "Invoices",
                columns: new[] { "ClientId", "IssuedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_EmployeeId",
                table: "Invoices",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_IssuedAt",
                table: "Invoices",
                column: "IssuedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Number",
                table: "Invoices",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TruckId",
                table: "Invoices",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_ProductId",
                table: "PurchaseItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_PurchaseId",
                table: "PurchaseItems",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_EmployeeId",
                table: "Purchases",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_Number",
                table: "Purchases",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_PurchasedAt",
                table: "Purchases",
                column: "PurchasedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TruckLoads_ProductId",
                table: "TruckLoads",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TruckLoads_TruckId_CreatedAt",
                table: "TruckLoads",
                columns: new[] { "TruckId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_Plate",
                table: "Trucks",
                column: "Plate",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TruckStock_ProductId",
                table: "TruckStock",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ClientEmptyBalances");

            migrationBuilder.DropTable(
                name: "ClientMovements");

            migrationBuilder.DropTable(
                name: "ConsignmentItems");

            migrationBuilder.DropTable(
                name: "DamagedProducts");

            migrationBuilder.DropTable(
                name: "DocumentCounters");

            migrationBuilder.DropTable(
                name: "InternalConsumptions");

            migrationBuilder.DropTable(
                name: "InvoiceEmptyGroups");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "PurchaseItems");

            migrationBuilder.DropTable(
                name: "TruckLoads");

            migrationBuilder.DropTable(
                name: "TruckStock");

            migrationBuilder.DropTable(
                name: "ConsignmentEvents");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Trucks");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropColumn(
                name: "AttendantName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CanChangeAttendant",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsGiftEligible",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Products");
        }
    }
}
