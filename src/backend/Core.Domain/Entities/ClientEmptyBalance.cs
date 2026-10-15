namespace Core.Domain.Entities;

/// <summary>
/// Saldo de vacíos de un cliente en un grupo de envase, con signo (negativo = debe).
/// Se guarda como dato estructurado para no tener que reconstruirlo desde el historial.
/// </summary>
public sealed class ClientEmptyBalance
{
    // Constructor para EF Core.
    private ClientEmptyBalance()
    {
    }

    internal ClientEmptyBalance(Guid clientId, string groupKey)
    {
        ClientId = clientId;
        GroupKey = groupKey;
    }

    public Guid ClientId { get; private set; }

    public string GroupKey { get; private set; } = default!;

    public int Boxes { get; private set; }

    public int Units { get; private set; }

    internal void Apply(int boxesDelta, int unitsDelta)
    {
        Boxes += boxesDelta;
        Units += unitsDelta;
    }
}
