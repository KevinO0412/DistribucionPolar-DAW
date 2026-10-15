namespace Core.Application.Abstractions;

/// <summary>
/// Política de crédito del negocio (configuración Credit:MaxOverdueDays): la deuda con más de esos días bloquea
/// las ventas a crédito del cliente.
/// </summary>
public sealed record CreditPolicy(int MaxOverdueDays = CreditPolicy.DefaultMaxOverdueDays)
{
    public const int DefaultMaxOverdueDays = 30;
}
