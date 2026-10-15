using Core.Domain.Exceptions;

namespace Core.Domain.ValueObjects;

/// <summary>
/// Cantidad expresada en cajas + unidades sueltas. El inventario siempre se guarda en unidades;
/// las cajas son solo una forma de captura y visualización.
/// </summary>
public readonly record struct BoxQuantity
{
    public BoxQuantity(int boxes, int looseUnits)
    {
        if (boxes < 0 || looseUnits < 0)
        {
            throw new DomainException("Las cajas y las unidades sueltas no pueden ser negativas.");
        }

        Boxes = boxes;
        LooseUnits = looseUnits;
    }

    public int Boxes { get; }

    public int LooseUnits { get; }

    public bool IsEmpty => Boxes == 0 && LooseUnits == 0;

    public int ToUnits(int unitsPerBox) => Boxes * unitsPerBox + LooseUnits;

    public static BoxQuantity FromUnits(int totalUnits, int unitsPerBox)
    {
        if (totalUnits < 0)
        {
            throw new DomainException("La cantidad total de unidades no puede ser negativa.");
        }

        return unitsPerBox <= 0
            ? new BoxQuantity(0, totalUnits)
            : new BoxQuantity(totalUnits / unitsPerBox, totalUnits % unitsPerBox);
    }

    public override string ToString() => (Boxes, LooseUnits) switch
    {
        (0, var units) => $"{units} und",
        (1, 0) => "1 caja",
        (var boxes, 0) => $"{boxes} cajas",
        (1, var units) => $"1 caja + {units} und",
        (var boxes, var units) => $"{boxes} cajas + {units} und"
    };
}
