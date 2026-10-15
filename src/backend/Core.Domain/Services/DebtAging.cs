namespace Core.Domain.Services;

/// <summary>Factura de venta con saldo pendiente: fecha local de emisión y monto que quedó por cobrar.</summary>
public readonly record struct OpenSale(DateOnly IssuedOn, decimal Pending);

/// <summary>Parte de la deuda con su antigüedad en días. <c>AgeDays</c> es null si no se puede atribuir a una factura.</summary>
public readonly record struct DebtPortion(int? AgeDays, decimal Amount);

/// <summary>Deuda de un cliente repartida por antigüedad, en los tramos de 0-30, 31-60, 61-90 y más de 90 días.</summary>
public sealed record DebtAgingResult(decimal Total, decimal Days0To30, decimal Days31To60, decimal Days61To90, decimal Over90,
    IReadOnlyList<DebtPortion> Portions)
{
    public static readonly DebtAgingResult Empty = new(0m, 0m, 0m, 0m, 0m, []);

    /// <summary>Monto con más de <paramref name="days"/> días (la deuda sin factura asociada cuenta como la más antigua).</summary>
    public decimal AmountOlderThan(int days) => Portions.Where(p => p.AgeDays is null || p.AgeDays > days).Sum(p => p.Amount);

    /// <summary>Antigüedad de la deuda más vieja; null si no hay deuda o no se puede fechar.</summary>
    public int? OldestDays => Portions.Count == 0 || Portions.Any(p => p.AgeDays is null) ? null : Portions.Max(p => p.AgeDays);
}

/// <summary>
/// Envejecimiento de la deuda con criterio FIFO: los abonos pagan primero las facturas más antiguas, así que la deuda
/// actual corresponde a los pendientes de las facturas más recientes. Se recorre de la más nueva a la más vieja
/// asignando deuda hasta agotarla. Si la deuda supera la suma de pendientes (por ejemplo, un ajuste manual de saldo),
/// el resto se considera más antiguo que cualquier factura y va al tramo de más de 90 días.
/// </summary>
public static class DebtAging
{
    public static DebtAgingResult Allocate(decimal debt, IEnumerable<OpenSale> openSales, DateOnly today)
    {
        if (debt <= 0)
        {
            return DebtAgingResult.Empty;
        }

        var portions = new List<DebtPortion>();
        decimal remaining = debt;
        foreach (var sale in openSales.Where(s => s.Pending > 0).OrderByDescending(s => s.IssuedOn))
        {
            if (remaining <= 0)
            {
                break;
            }

            decimal amount = Math.Min(sale.Pending, remaining);
            portions.Add(new DebtPortion(Math.Max(0, today.DayNumber - sale.IssuedOn.DayNumber), amount));
            remaining -= amount;
        }

        if (remaining > 0)
        {
            portions.Add(new DebtPortion(null, remaining));
        }

        decimal Sum(Func<int?, bool> inBucket) => portions.Where(p => inBucket(p.AgeDays)).Sum(p => p.Amount);
        return new DebtAgingResult(
            debt,
            Sum(age => age is <= 30),
            Sum(age => age is > 30 and <= 60),
            Sum(age => age is > 60 and <= 90),
            Sum(age => age is null or > 90),
            portions);
    }
}
