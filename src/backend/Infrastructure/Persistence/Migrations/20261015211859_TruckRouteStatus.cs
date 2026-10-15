using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TruckRouteStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DispatchedAt",
                table: "Trucks",
                type: "timestamptz",
                nullable: true);

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

            // Camiones que ya tenían carga antes de los estados de ruta: quedan "Cargando" (listos para despacharse).
            migrationBuilder.Sql(
                "UPDATE \"Trucks\" t SET \"Status\" = 'Cargando' " +
                "WHERE t.\"Status\" = 'En galpón' AND EXISTS (SELECT 1 FROM \"TruckStock\" s WHERE s.\"TruckId\" = t.\"Id\");");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Trucks_Status",
                table: "Trucks",
                sql: "\"Status\" IN ('En galpón', 'Cargando', 'En ruta', 'Liquidado')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Trucks_Status",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "DispatchedAt",
                table: "Trucks");
        }
    }
}
