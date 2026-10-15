using Core.Domain.Common;
using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Cliente de la distribuidora. Los saldos son CON SIGNO: negativo = el cliente debe;
/// positivo = saldo a favor del cliente; cero = sin saldo. Aplica al dinero y a los vacíos.
/// El código, el nombre y el RIF son inmutables una vez creado el cliente.
/// </summary>
public sealed class Client : BaseEntity
{
    public const int RifMaxLength = 20;
    public const int NameMaxLength = 150;
    public const int AddressMaxLength = 250;
    public const int NicknameMaxLength = 100;
    public const int TypeMaxLength = 30;
    public const string DefaultAddress = "Cliente ocasional / sin dirección registrada";
    public const string DefaultType = "Ocasional";

    private readonly List<ClientEmptyBalance> emptyBalances = [];

    // Constructor para EF Core.
    private Client()
    {
    }

    public Client(int code, string rif, string name, string? address, string? nickname, string? type)
    {
        Code = Guard.Positive(code, "código del cliente");
        Rif = Guard.RequiredText(rif, "RIF o cédula", RifMaxLength).ToUpperInvariant();
        Name = Guard.RequiredText(name, "nombre del cliente", NameMaxLength);
        ApplyContact(address, nickname, type);
    }

    /// <summary>Código correlativo del cliente (se muestra con 3 dígitos: 001, 002…).</summary>
    public int Code { get; private set; }

    public string Rif { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public string Address { get; private set; } = default!;

    public string? Nickname { get; private set; }

    public string Type { get; private set; } = default!;

    /// <summary>Saldo de dinero con signo (negativo = debe).</summary>
    public decimal MoneyBalance { get; private set; }

    /// <summary>Saldo de cajas de vacíos con signo (negativo = debe).</summary>
    public int EmptyBoxesBalance { get; private set; }

    /// <summary>Saldo de unidades de vacíos con signo (negativo = debe).</summary>
    public int EmptyUnitsBalance { get; private set; }

    public IReadOnlyCollection<ClientEmptyBalance> EmptyBalances => emptyBalances.AsReadOnly();

    /// <summary>Límite de crédito en USD; null = sin límite, 0 = solo de contado.</summary>
    public decimal? CreditLimit { get; private set; }

    /// <summary>Lo que el cliente debe (0 si su saldo está limpio o a favor).</summary>
    public decimal Debt => MoneyBalance < 0 ? -MoneyBalance : 0m;

    public string FormattedCode => Code.ToString("D3");

    /// <summary>Tiene alguna deuda pendiente (dinero o vacíos).</summary>
    public bool IsReceivable => MoneyBalance < 0 || EmptyBoxesBalance < 0 || EmptyUnitsBalance < 0;

    public void UpdateContact(string? address, string? nickname, string? type)
    {
        ApplyContact(address, nickname, type);
        Touch();
    }

    /// <summary>Carga a la cuenta lo que quedó pendiente de una factura (un pendiente negativo deja saldo a favor).</summary>
    public void ApplyInvoicePending(decimal pending)
    {
        MoneyBalance -= pending;
        Touch();
    }

    /// <summary>Revierte el pendiente de una factura anulada (lo devuelve al saldo).</summary>
    public void RevertInvoicePending(decimal pending)
    {
        MoneyBalance += pending;
        Touch();
    }

    /// <summary>Abono de dinero fuera de factura.</summary>
    public void ApplyPayment(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("El monto del abono debe ser mayor que 0.");
        }

        MoneyBalance += amount;
        Touch();
    }

    /// <summary>
    /// Aplica un movimiento de vacíos de un grupo: <paramref name="boxesDelta"/> y <paramref name="unitsDelta"/>
    /// se suman al saldo (negativo = el cliente pasa a deber más vacíos).
    /// </summary>
    public void ApplyEmptiesMovement(string groupKey, int boxesDelta, int unitsDelta)
    {
        if (!EmptyReturnGroups.Exists(groupKey))
        {
            throw new DomainException($"El grupo de vacíos '{groupKey}' no existe.");
        }

        if (boxesDelta == 0 && unitsDelta == 0)
        {
            return;
        }

        var balance = emptyBalances.FirstOrDefault(b => b.GroupKey == groupKey);
        if (balance is null)
        {
            balance = new ClientEmptyBalance(Id, groupKey);
            emptyBalances.Add(balance);
        }

        balance.Apply(boxesDelta, unitsDelta);
        EmptyBoxesBalance += boxesDelta;
        EmptyUnitsBalance += unitsDelta;
        Touch();
    }

    /// <summary>Ajuste manual (solo administración): fija valores absolutos de los saldos totales.</summary>
    public void SetCreditLimit(decimal? limit)
    {
        if (limit is < 0)
        {
            throw new DomainException("El límite de crédito no puede ser negativo.");
        }

        CreditLimit = limit is null ? null : decimal.Round(limit.Value, 2);
        Touch();
    }

    /// <summary>
    /// Control de crédito: una venta que deja deuda (<paramref name="newPending"/> &gt; 0) se rechaza si el cliente tiene
    /// deuda vencida o si supera su límite. Las ventas de contado siempre se permiten.
    /// </summary>
    public void EnsureCreditAvailable(decimal newPending, decimal overdueDebt, int maxOverdueDays)
    {
        if (newPending <= 0)
        {
            return;
        }

        if (overdueDebt > 0)
        {
            throw new DomainException(
                $"Crédito bloqueado: {Name} tiene {Usd(overdueDebt)} de deuda con más de {maxOverdueDays} días. " +
                "Registre un abono o cobre la venta completa (de contado).");
        }

        if (CreditLimit is { } limit && Debt + newPending > limit)
        {
            throw new DomainException(
                $"Crédito bloqueado: la venta dejaría a {Name} con {Usd(Debt + newPending)} de deuda y su límite es {Usd(limit)}. " +
                $"Cobre al menos {Usd(Debt + newPending - limit)} en esta factura.");
        }
    }

    private static string Usd(decimal value) => "$" + value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

    public void AdjustBalances(decimal moneyBalance, int emptyBoxesBalance, int emptyUnitsBalance)
    {
        MoneyBalance = moneyBalance;
        EmptyBoxesBalance = emptyBoxesBalance;
        EmptyUnitsBalance = emptyUnitsBalance;
        Touch();
    }

    private void ApplyContact(string? address, string? nickname, string? type)
    {
        Address = Guard.OptionalText(address, "dirección", AddressMaxLength) ?? DefaultAddress;
        Nickname = Guard.OptionalText(nickname, "referencia", NicknameMaxLength);
        Type = Guard.OptionalText(type, "tipo de cliente", TypeMaxLength) ?? DefaultType;
    }
}
