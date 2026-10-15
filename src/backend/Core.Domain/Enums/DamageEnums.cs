namespace Core.Domain.Enums;

/// <summary>Clasificación de una avería.</summary>
public enum DamageType
{
    Damaged = 1,
    Unfit = 2,
    Discarded = 3
}

/// <summary>Causa de una avería.</summary>
public enum DamageReason
{
    Expired = 1,
    Broken = 2,
    Dented = 3,
    Spilled = 4,
    Wet = 5,
    OpenPackage = 6,
    Other = 7
}

/// <summary>Destino que se le dio al producto averiado.</summary>
public enum DamageAction
{
    Discarded = 1,
    InternalConsumption = 2,
    Gifted = 3,
    ReturnedToSupplier = 4
}
