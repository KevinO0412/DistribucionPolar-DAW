using Core.Domain.Enums;

namespace Core.Application.DTOs;

/// <summary>Cliente con sus saldos. Convención de signo: negativo = debe; positivo = a favor.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Code">Código correlativo con 3 dígitos (001…).</param>
/// <param name="Rif">RIF o cédula.</param>
/// <param name="Name">Nombre o razón social.</param>
/// <param name="Address">Dirección.</param>
/// <param name="Nickname">Referencia o apodo.</param>
/// <param name="Type">Tipo de cliente.</param>
/// <param name="MoneyBalance">Saldo de dinero con signo.</param>
/// <param name="MoneyStatus">"Debe: $X", "Saldo a favor: $X" o "Sin saldo".</param>
/// <param name="EmptyBoxesBalance">Saldo de cajas de vacíos con signo.</param>
/// <param name="EmptyUnitsBalance">Saldo de unidades de vacíos con signo.</param>
/// <param name="EmptiesStatus">Texto del saldo de vacíos.</param>
/// <param name="EmptyBalances">Saldo de vacíos por grupo de envase.</param>
/// <param name="IsReceivable">Tiene alguna deuda (dinero o vacíos).</param>
/// <param name="LastPurchaseAt">Fecha de la última factura de venta vigente.</param>
/// <param name="CreatedAt">Fecha de alta (UTC).</param>
public sealed record ClientResponse(
    Guid Id, string Code, string Rif, string Name, string Address, string? Nickname, string Type,
    decimal MoneyBalance, string MoneyStatus, int EmptyBoxesBalance, int EmptyUnitsBalance, string EmptiesStatus,
    IReadOnlyList<EmptyGroupBalance> EmptyBalances, bool IsReceivable, DateTime? LastPurchaseAt, DateTime CreatedAt);

/// <summary>Alta de cliente. Se crea como "Ocasional" con saldos en 0.</summary>
/// <param name="Rif" example="V-12345678">RIF o cédula, obligatorio y único.</param>
/// <param name="Name" example="Bodega La Esquina">Nombre, obligatorio.</param>
/// <param name="Address" example="Av. Principal, local 3">Dirección; si se omite, "Cliente ocasional / sin dirección registrada".</param>
/// <param name="Nickname" example="La Esquina">Referencia o apodo.</param>
public sealed record CreateClientRequest(string Rif, string Name, string? Address, string? Nickname);

/// <summary>Edición de cliente. El código, el nombre y el RIF son inmutables.</summary>
/// <param name="Address">Dirección.</param>
/// <param name="Nickname">Referencia o apodo.</param>
/// <param name="Type" example="ESPECIAL">Tipo de cliente.</param>
public sealed record UpdateClientRequest(string? Address, string? Nickname, string? Type);

/// <summary>Abono de dinero fuera de factura.</summary>
/// <param name="Amount" example="25.00">Monto mayor que 0, máximo 2 decimales.</param>
/// <param name="Observation" example="Pago en efectivo">Observación opcional.</param>
public sealed record ClientPaymentRequest(decimal Amount, string? Observation);

/// <summary>Devolución de vacíos fuera de factura.</summary>
/// <param name="Groups">Cajas y unidades devueltas por grupo (no negativas; al menos una mayor que 0).</param>
/// <param name="Observation">Observación opcional.</param>
public sealed record ClientEmptiesReturnRequest(IReadOnlyList<EmptyGroupQuantity> Groups, string? Observation);

/// <summary>Ajuste manual de saldos (solo Admin): fija valores absolutos con signo.</summary>
/// <param name="MoneyBalance" example="-60.00">Saldo de dinero.</param>
/// <param name="EmptyBoxesBalance" example="-1">Saldo de cajas de vacíos.</param>
/// <param name="EmptyUnitsBalance" example="-5">Saldo de unidades de vacíos.</param>
/// <param name="Reason" example="Conciliación con el cliente">Motivo, obligatorio.</param>
public sealed record ClientBalanceAdjustmentRequest(decimal MoneyBalance, int EmptyBoxesBalance, int EmptyUnitsBalance, string Reason);

/// <summary>Movimiento del estado de cuenta.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Type">Tipo de movimiento.</param>
/// <param name="Title">Título (por ejemplo, el número de factura).</param>
/// <param name="Description">Detalle.</param>
/// <param name="Amount">Monto.</param>
/// <param name="InvoiceId">Factura relacionada, si aplica.</param>
/// <param name="Date">Fecha (UTC).</param>
public sealed record ClientMovementResponse(Guid Id, ClientMovementType Type, string Title, string Description, decimal Amount, Guid? InvoiceId, DateTime Date);

/// <summary>Estado de cuenta del cliente, separado por pestañas (de más reciente a más antiguo).</summary>
/// <param name="Client">Cliente y saldos.</param>
/// <param name="Invoices">Facturas.</param>
/// <param name="Gifts">Regalías.</param>
/// <param name="Payments">Abonos.</param>
/// <param name="Empties">Vacíos generados, devueltos y anulaciones.</param>
/// <param name="Adjustments">Ajustes manuales de saldo.</param>
public sealed record ClientStatementResponse(
    ClientResponse Client,
    IReadOnlyList<ClientMovementResponse> Invoices,
    IReadOnlyList<ClientMovementResponse> Gifts,
    IReadOnlyList<ClientMovementResponse> Payments,
    IReadOnlyList<ClientMovementResponse> Empties,
    IReadOnlyList<ClientMovementResponse> Adjustments);
