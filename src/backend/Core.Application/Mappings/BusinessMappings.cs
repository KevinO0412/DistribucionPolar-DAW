using System.Globalization;
using Core.Application.DTOs;
using Core.Domain.Common;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;

namespace Core.Application.Mappings;

internal static class BusinessMappings
{
    public static string Money(decimal value) => "$" + value.ToString("0.00", CultureInfo.InvariantCulture);

    /// <summary>"Debe: $X", "Saldo a favor: $X" o "Sin saldo" (negativo = debe).</summary>
    public static string MoneyStatus(decimal balance) => balance switch
    {
        < 0 => $"Debe: {Money(-balance)}",
        > 0 => $"Saldo a favor: {Money(balance)}",
        _ => "Sin saldo"
    };

    public static string EmptiesText(int boxes, int units) => $"{boxes} cajas + {units} und";

    /// <summary>"Debe vacíos: …" si cajas o unidades son negativas; "Vacíos a favor: …" si son positivas.</summary>
    public static string EmptiesStatus(int boxes, int units)
    {
        if (boxes < 0 || units < 0)
        {
            return $"Debe vacíos: {EmptiesText(Math.Max(0, -boxes), Math.Max(0, -units))}";
        }

        return boxes > 0 || units > 0 ? $"Vacíos a favor: {EmptiesText(boxes, units)}" : "Sin vacíos pendientes";
    }

    public static EmployeeResponse ToResponse(this Employee employee) =>
        new(employee.Id, employee.Name, employee.Role, employee.IsActive, employee.CreatedAt);

    public static TruckResponse ToResponse(this Truck truck)
    {
        var lines = truck.Stock
            .Select(s => new TruckStockLineResponse(
                s.ProductId,
                s.Product?.SKU ?? string.Empty,
                s.Product?.Name ?? string.Empty,
                s.Product?.Category?.Name,
                s.Product?.UnitsPerBox ?? 0,
                s.StockUnits,
                BoxQuantity.FromUnits(s.StockUnits, s.Product?.UnitsPerBox ?? 0).ToString(),
                s.Product?.PriceUnit ?? 0,
                s.StockUnits * (s.Product?.PriceUnit ?? 0)))
            .OrderBy(l => l.ProductName)
            .ToList();

        return new TruckResponse(
            truck.Id, truck.Name, truck.Plate, truck.Status, truck.TotalUnits, lines.Sum(l => l.Value), lines,
            truck.CapacityBoxes, Math.Round(truck.LoadedBoxes, 1), truck.OccupancyPercent, truck.DispatchedAt);
    }

    public static TruckLoadResponse ToResponse(this TruckLoad load) => new(
        load.Id, load.TruckId, load.TruckName, load.ProductId, load.ProductName, load.TotalUnits,
        load.StockBefore, load.StockAfter, load.Source, load.Observation, load.Username, load.CreatedAt);

    public static ClientResponse ToResponse(this Client client, DateTime? lastPurchaseAt = null) => new(
        client.Id,
        client.FormattedCode,
        client.Rif,
        client.Name,
        client.Address,
        client.Nickname,
        client.Type,
        client.MoneyBalance,
        MoneyStatus(client.MoneyBalance),
        client.EmptyBoxesBalance,
        client.EmptyUnitsBalance,
        EmptiesStatus(client.EmptyBoxesBalance, client.EmptyUnitsBalance),
        client.EmptyBalances
            .Where(b => b.Boxes != 0 || b.Units != 0)
            .OrderBy(b => b.GroupKey)
            .Select(b => new EmptyGroupBalance(b.GroupKey, EmptyReturnGroups.GetName(b.GroupKey), b.Boxes, b.Units))
            .ToList(),
        client.IsReceivable,
        lastPurchaseAt,
        client.CreatedAt, client.CreditLimit);

    public static ClientMovementResponse ToResponse(this ClientMovement movement) => new(
        movement.Id, movement.Type, movement.Title, movement.Description, movement.Amount, movement.InvoiceId, movement.CreatedAt);

    public static InvoiceResponse ToResponse(this Invoice invoice) => new(
        invoice.Id,
        invoice.Number,
        invoice.Type,
        invoice.ClientId,
        invoice.ClientName,
        invoice.ClientRif,
        invoice.IssuedAt,
        invoice.DispatchOrigin,
        invoice.TruckId,
        invoice.TruckName,
        invoice.TruckPlate,
        invoice.AttendantName,
        invoice.Total,
        invoice.Payment,
        invoice.Pending,
        invoice.PaymentObservation,
        invoice.GeneratedEmptyBoxes,
        invoice.GeneratedEmptyUnits,
        invoice.ReturnedEmptyBoxes,
        invoice.ReturnedEmptyUnits,
        invoice.Items
            .Select(i => new InvoiceItemResponse(
                i.ProductId, i.SKU, i.ProductName, i.PriceBox, i.PriceUnit, i.UnitsPerBox,
                i.Boxes, i.LooseUnits, i.TotalUnits, i.Quantity.ToString(), i.Subtotal))
            .ToList(),
        invoice.EmptyGroups
            .OrderBy(g => g.GroupKey)
            .Select(g => new InvoiceEmptyGroupResponse(
                g.GroupKey, EmptyReturnGroups.GetName(g.GroupKey), g.GeneratedBoxes, g.GeneratedUnits,
                g.ReturnedBoxes, g.ReturnedUnits, g.PendingBoxes, g.PendingUnits))
            .ToList(),
        invoice.IsCancelled,
        invoice.CancellationReason,
        invoice.CancelledAt,
        invoice.CancelledBy);

    public static PurchaseResponse ToResponse(this Purchase purchase) => new(
        purchase.Id,
        purchase.Number,
        purchase.PurchasedAt,
        purchase.EmployeeName,
        purchase.DocumentType,
        purchase.DocumentNumber,
        purchase.TotalUnits,
        purchase.TotalAmount,
        purchase.Items
            .Select(i => new PurchaseItemResponse(i.ProductId, i.ProductName, i.Boxes, i.LooseUnits, i.TotalUnits, i.Subtotal))
            .ToList());

    public static DamageResponse ToResponse(this DamagedProduct damage) => new(
        damage.Id, damage.OccurredAt, damage.Type, damage.Reason, damage.ActionTaken, damage.Origin, damage.TruckName,
        damage.ProductId, damage.ProductName, damage.Boxes, damage.LooseUnits, damage.TotalUnits, damage.EstimatedLoss, damage.Observation);

    public static InternalConsumptionResponse ToResponse(this InternalConsumption consumption) => new(
        consumption.Id, consumption.OccurredAt, consumption.Type, consumption.EmployeeName, consumption.Origin, consumption.TruckName,
        consumption.ProductId, consumption.ProductName, consumption.Boxes, consumption.LooseUnits, consumption.TotalUnits,
        consumption.EstimatedValue, consumption.Observation);

    public static ConsignmentResponse ToResponse(this ConsignmentEvent consignment) => new(
        consignment.Id,
        consignment.Name,
        consignment.Responsible,
        consignment.EventDate,
        consignment.IsClosed,
        consignment.ClosedAt,
        consignment.TotalSold,
        consignment.Items
            .OrderBy(i => i.ProductName)
            .Select(i => new ConsignmentItemResponse(
                i.ProductId, i.ProductName, i.UnitsPerBox, i.DeliveredUnits, i.ReturnedUnits, i.PendingUnits, i.SoldUnits,
                BoxQuantity.FromUnits(i.SoldUnits, i.UnitsPerBox).ToString(), i.SoldAmount))
            .ToList());

    public static UserResponse ToResponse(this User user) => new(
        user.Id, user.Username, user.Email, user.FullName, user.Role, user.IsActive, user.AttendantName, user.CanChangeAttendant, user.CreatedAt);

    public static RouteSettlementResponse ToResponse(this RouteSettlement settlement) => new(
        settlement.Id, settlement.TruckId, settlement.TruckName, settlement.TruckPlate, settlement.DispatchedAt, settlement.SettledAt,
        settlement.SettledBy, settlement.InvoicesCount, settlement.SalesTotal, settlement.ExpectedCash, settlement.CountedCash,
        settlement.CashDifference, settlement.MissingGoodsValue, settlement.IsBalanced, settlement.Observation,
        settlement.Lines
            .OrderBy(l => l.DifferenceUnits)
            .ThenBy(l => l.ProductName)
            .Select(l => new RouteSettlementLineResponse(
                l.ProductId, l.ProductName, l.UnitsPerBox,
                l.ExpectedUnits, BoxQuantity.FromUnits(l.ExpectedUnits, l.UnitsPerBox).ToString(),
                l.CountedUnits, BoxQuantity.FromUnits(l.CountedUnits, l.UnitsPerBox).ToString(),
                l.DifferenceUnits, l.DifferenceValue))
            .ToList());
}
