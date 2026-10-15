using System.Globalization;
using Core.Domain.Entities;
using Core.Domain.Exceptions;

namespace Core.Domain.Services;

/// <summary>Situación de crédito de un cliente y si puede comprar a crédito.</summary>
public sealed record CreditStatus(
    decimal Debt,
    decimal? CreditLimit,
    int CreditDays,
    decimal OverdueAmount,
    int? OldestDebtDays,
    decimal? Available,
    bool IsBlocked,
    string? BlockReason,
    DebtAgingResult Aging);

/// <summary>
/// Control de crédito: se bloquean las ventas que dejan saldo pendiente (a crédito) cuando el cliente tiene deuda vencida
/// (más días que sus días de crédito) o cuando la deuda resultante excede su límite. Las ventas de contado se permiten.
/// </summary>
public static class CreditPolicy
{
    private static string Money(decimal? amount) => (amount ?? 0m).ToString("0.00", CultureInfo.InvariantCulture);

    public static CreditStatus Evaluate(Client client, IEnumerable<OpenSale> openSales, DateOnly today)
    {
        decimal debt = Math.Max(0m, -client.MoneyBalance);
        var aging = DebtAging.Allocate(debt, openSales, today);
        decimal overdue = aging.AmountOlderThan(client.CreditDays);
        decimal? available = client.CreditLimit is null ? null : Math.Max(0m, client.CreditLimit.Value - debt);

        string? reason = overdue > 0
            ? $"El cliente {client.Name} tiene ${Money(overdue)} de deuda vencida (más de {client.CreditDays} días). Solo puede comprar de contado."
            : client.CreditLimit is not null && debt >= client.CreditLimit
                ? $"El cliente {client.Name} alcanzó su límite de crédito de ${Money(client.CreditLimit)} (debe ${Money(debt)}). Solo puede comprar de contado."
                : null;

        return new CreditStatus(debt, client.CreditLimit, client.CreditDays, overdue, aging.OldestDays, available, reason is not null, reason, aging);
    }

    /// <summary>Lanza <see cref="DomainException"/> si la venta deja saldo pendiente y el crédito no lo permite.</summary>
    public static void EnsureCanSell(Client client, IEnumerable<OpenSale> openSales, DateOnly today, decimal newPending)
    {
        if (newPending <= 0)
        {
            return;
        }

        var status = Evaluate(client, openSales, today);
        if (status.OverdueAmount > 0)
        {
            throw new DomainException(status.BlockReason!);
        }

        if (client.CreditLimit is not null && status.Debt + newPending > client.CreditLimit)
        {
            throw new DomainException(
                $"La venta deja ${Money(newPending)} pendiente y supera el límite de crédito de {client.Name}: " +
                $"debe ${Money(status.Debt)}, límite ${Money(client.CreditLimit)}, disponible ${Money(status.Available)}.");
        }
    }
}
