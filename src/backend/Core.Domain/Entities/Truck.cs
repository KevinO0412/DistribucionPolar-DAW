using Core.Domain.Common;
using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Camión de reparto con su propio inventario. Cada línea guarda unidades de un producto;
/// una línea que llega a 0 se elimina.
/// </summary>
public sealed class Truck : BaseEntity
{
    public const int NameMaxLength = 50;
    public const int PlateMaxLength = 20;
    public const int StatusMaxLength = 30;
    public const string DefaultStatus = StatusInWarehouse;

    // Estados de la ruta: En galpón → Cargando → En ruta → Liquidado → (nueva carga) Cargando…
    public const string StatusInWarehouse = "En galpón";
    public const string StatusLoading = "Cargando";
    public const string StatusOnRoute = "En ruta";
    public const string StatusSettled = "Liquidado";
    public const int DefaultCapacityBoxes = 500;
    public const int MaxCapacityBoxes = 5000;

    private readonly List<TruckStock> stock = [];

    // Constructor para EF Core.
    private Truck()
    {
    }

    public Truck(string name, string plate, int capacityBoxes = DefaultCapacityBoxes)
    {
        Apply(name, plate, capacityBoxes);
        Status = DefaultStatus;
    }

    public string Name { get; private set; } = default!;

    public string Plate { get; private set; } = default!;

    public string Status { get; private set; } = default!;

    /// <summary>Salida a ruta vigente; marca el inicio del período que se liquida.</summary>
    public DateTime? DispatchedAt { get; private set; }

    public bool IsOnRoute => Status == StatusOnRoute;

    /// <summary>Capacidad de carga en cajas: base del KPI de ocupación de flota.</summary>
    public int CapacityBoxes { get; private set; }

    public IReadOnlyCollection<TruckStock> Stock => stock.AsReadOnly();

    public int TotalUnits => stock.Sum(s => s.StockUnits);

    public void Update(string name, string plate, int capacityBoxes)
    {
        Apply(name, plate, capacityBoxes);
        Touch();
    }

    public int GetUnits(Guid productId) => stock.FirstOrDefault(s => s.ProductId == productId)?.StockUnits ?? 0;

    public void AddStock(Guid productId, int units)
    {
        Guard.Positive(units, "cantidad a cargar");
        var line = stock.FirstOrDefault(s => s.ProductId == productId);
        if (line is null)
        {
            stock.Add(new TruckStock(Id, productId, units));
        }
        else
        {
            line.Increase(units);
        }

        Touch();
    }

    public void RemoveStock(Guid productId, int units)
    {
        Guard.Positive(units, "cantidad a retirar");
        var line = stock.FirstOrDefault(s => s.ProductId == productId);
        int available = line?.StockUnits ?? 0;
        if (line is null || units > available)
        {
            throw new DomainException(
                $"Inventario insuficiente en el camión {Name}: se solicitan {units} und y hay {available} und.");
        }

        line.Decrease(units);
        if (line.StockUnits == 0)
        {
            stock.Remove(line);
        }

        Touch();
    }

    /// <summary>Una carga desde el galpón deja el camión "Cargando", salvo que ya esté en ruta.</summary>
    public void MarkLoading()
    {
        if (!IsOnRoute)
        {
            Status = StatusLoading;
            Touch();
        }
    }

    /// <summary>Sale a ruta con la mercancía cargada.</summary>
    public void Dispatch(DateTime dispatchedAt)
    {
        if (IsOnRoute)
        {
            throw new DomainException($"El camión {Name} ya está en ruta.");
        }

        if (TotalUnits == 0)
        {
            throw new DomainException($"El camión {Name} no tiene mercancía cargada.");
        }

        Status = StatusOnRoute;
        DispatchedAt = dispatchedAt;
        Touch();
    }

    /// <summary>Cierra la ruta: retira la mercancía (se reintegra al galpón según el conteo) y queda "Liquidado".</summary>
    public IReadOnlyList<(Guid ProductId, int Units)> Settle()
    {
        if (!IsOnRoute)
        {
            throw new DomainException($"Solo se liquida un camión en ruta; {Name} está \"{Status}\".");
        }

        var expected = Unload();
        Status = StatusSettled;
        return expected;
    }

    /// <summary>Fija la cantidad absoluta de un producto (ajuste manual). En 0 elimina la línea.</summary>
    public void SetStock(Guid productId, int units)
    {
        Guard.NotNegative(units, "cantidad del camión");
        var line = stock.FirstOrDefault(s => s.ProductId == productId);
        if (units == 0)
        {
            if (line is not null)
            {
                stock.Remove(line);
            }
        }
        else if (line is null)
        {
            stock.Add(new TruckStock(Id, productId, units));
        }
        else
        {
            line.Set(units);
        }

        Touch();
    }

    /// <summary>Retira todas las líneas del camión y las devuelve para reintegrarlas al galpón.</summary>
    public IReadOnlyList<(Guid ProductId, int Units)> Unload()
    {
        var unloaded = stock.Select(s => (s.ProductId, s.StockUnits)).ToList();
        stock.Clear();
        Touch();
        return unloaded;
    }

    /// <summary>
    /// Ocupación en cajas equivalentes (unidades / unidades por caja) frente a la capacidad.
    /// Requiere los productos cargados en las líneas; puede superar el 100 % si el camión va sobrecargado.
    /// </summary>
    public decimal LoadedBoxes => stock.Sum(s => s.Product is { UnitsPerBox: > 0 } p ? (decimal)s.StockUnits / p.UnitsPerBox : 0);

    public decimal OccupancyPercent => CapacityBoxes > 0 ? Math.Round(LoadedBoxes * 100 / CapacityBoxes, 1) : 0;

    private void Apply(string name, string plate, int capacityBoxes)
    {
        Name = Guard.RequiredText(name, "nombre del camión", NameMaxLength);
        Plate = Guard.RequiredText(plate, "placa", PlateMaxLength).ToUpperInvariant();
        if (capacityBoxes <= 0 || capacityBoxes > MaxCapacityBoxes)
        {
            throw new DomainException($"La capacidad del camión debe estar entre 1 y {MaxCapacityBoxes} cajas.");
        }

        CapacityBoxes = capacityBoxes;
    }
}
