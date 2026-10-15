namespace Core.Domain.Services;

/// <summary>Venta con saldo pendiente: fecha de emisión y monto que quedó por cobrar.</summary>
public readonly record struct PendingSale(DateTime IssuedAtUtc, decimal Pending);

/// <summary>Parte de la deuda con su antigüedad en días. <see cref="DebtAging.UnexplainedAge"/> = sin factura asociada.</summary>
public readonly record struct AgedAmount(int AgeDays, decimal Amount);

/// <summary>
/// Antigüedad de la deuda de un cliente. El sistema lleva un saldo único por cliente, así que los abonos se
/// imputan primero a lo más antiguo (FIFO): la deuda actual corresponde a los pendientes más recientes.
/// La parte que ninguna factura explica (saldo inicial o ajuste manual) se considera la más antigua.
/// </summary>
public static class DebtAging
{
    public const int UnexplainedAge = int.MaxValue;

    public static IReadOnlyList<AgedAmount> Allocate(decimal debt, IEnumerable<PendingSale> sales, DateTime nowUtc)
    {
        var result = new List<AgedAmount>();
        decimal remaining = debt;
        if (remaining <= 0)
        {
            return result;
        }

        foreach (var sale in sales.Where(s => s.Pending > 0).OrderByDescending(s => s.IssuedAtUtc))
        {
            if (remaining <= 0)
            {
                break;
            }

            decimal take = Math.Min(remaining, sale.Pending);
            result.Add(new AgedAmount(Math.Max(0, (int)(nowUtc - sale.IssuedAtUtc).TotalDays), take));
            remaining -= take;
        }

        if (remaining > 0)
        {
            result.Add(new AgedAmount(UnexplainedAge, remaining));
        }

        return result;
    }

    /// <summary>Parte de la deuda con más de <paramref name="maxDays"/> días.</summary>
    public static decimal Overdue(decimal debt, IEnumerable<PendingSale> sales, DateTime nowUtc, int maxDays) =>
        Allocate(debt, sales, nowUtc).Where(a => a.AgeDays > maxDays).Sum(a => a.Amount);
}
