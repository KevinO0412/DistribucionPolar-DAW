using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TruckCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CapacityBoxes",
                table: "Trucks",
                type: "integer",
                nullable: false,
                defaultValue: 500);

            migrationBuilder.UpdateData(
                table: "Trucks",
                keyColumn: "Id",
                keyValue: new Guid("00000200-0000-4000-8000-000000000001"),
                column: "CapacityBoxes",
                value: 600);

            migrationBuilder.UpdateData(
                table: "Trucks",
                keyColumn: "Id",
                keyValue: new Guid("00000200-0000-4000-8000-000000000002"),
                column: "CapacityBoxes",
                value: 500);

            migrationBuilder.UpdateData(
                table: "Trucks",
                keyColumn: "Id",
                keyValue: new Guid("00000200-0000-4000-8000-000000000003"),
                column: "CapacityBoxes",
                value: 900);

            migrationBuilder.UpdateData(
                table: "Trucks",
                keyColumn: "Id",
                keyValue: new Guid("00000200-0000-4000-8000-000000000004"),
                column: "CapacityBoxes",
                value: 700);

            migrationBuilder.UpdateData(
                table: "Trucks",
                keyColumn: "Id",
                keyValue: new Guid("00000200-0000-4000-8000-000000000005"),
                column: "CapacityBoxes",
                value: 150);

            migrationBuilder.UpdateData(
                table: "Trucks",
                keyColumn: "Id",
                keyValue: new Guid("00000200-0000-4000-8000-000000000006"),
                column: "CapacityBoxes",
                value: 60);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Trucks_CapacityBoxes_Range",
                table: "Trucks",
                sql: "\"CapacityBoxes\" BETWEEN 1 AND 5000");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Trucks_CapacityBoxes_Range",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "CapacityBoxes",
                table: "Trucks");
        }
    }
}
