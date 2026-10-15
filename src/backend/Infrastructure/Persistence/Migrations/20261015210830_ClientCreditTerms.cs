using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClientCreditTerms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreditDays",
                table: "Clients",
                type: "integer",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                table: "Clients",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("00000300-0000-4000-8000-000000000001"),
                columns: new[] { "CreditDays", "CreditLimit" },
                values: new object[] { 30, null });

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("00000300-0000-4000-8000-000000000002"),
                columns: new[] { "CreditDays", "CreditLimit" },
                values: new object[] { 30, null });

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("00000300-0000-4000-8000-000000000003"),
                columns: new[] { "CreditDays", "CreditLimit" },
                values: new object[] { 30, null });

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("00000300-0000-4000-8000-000000000004"),
                columns: new[] { "CreditDays", "CreditLimit" },
                values: new object[] { 30, null });

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: new Guid("00000300-0000-4000-8000-000000000005"),
                columns: new[] { "CreditDays", "CreditLimit" },
                values: new object[] { 30, null });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Clients_Credit",
                table: "Clients",
                sql: "\"CreditLimit\" IS NULL OR \"CreditLimit\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Clients_Credit",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CreditDays",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CreditLimit",
                table: "Clients");
        }
    }
}
