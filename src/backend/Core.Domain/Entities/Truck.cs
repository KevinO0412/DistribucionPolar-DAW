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

    /// <summary>Sin carga, en el galpón.</summary>
    public const string StatusInWarehouse = "En galpón";

    /// <summary>Se le está cargando mercancía antes de salir.</summary>
    public const string StatusLoading = "Cargando";

    /// <summary>Salió a ruta: vende desde su inventario.</summary>
    public const string StatusOnRoute = "En ruta";

    /// <summary>Volvió y se hizo la liquidación ciega de la ruta.</summary>
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

    /// <summary>En galpón → Cargando → En ruta → Liquidado → (descarga) En galpón.</summary>
    public string Status { get; private set; } = default!;

    /// <summary>Salida a la ruta actual (o la última); las ventas de la ruta se cuentan desde aquí.</summary>
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

    /// <summary>Retira todas las líneas del camión y las devuelve para reintegrarlas al galpón. Queda "En galpón".</summary>
    public IReadOnlyList<(Guid ProductId, int Units)> Unload()
    {
        var unloaded = stock.Select(s => (s.ProductId, s.StockUnits)).ToList();
        stock.Clear();
        Status = StatusInWarehouse;
        Touch();
        return unloaded;
    }

    /// <summary>Una carga desde el galpón lo pone "Cargando"; si ya está en ruta (reabastecimiento) sigue en ruta.</summary>
    public void MarkLoading()
    {
        if (Status != StatusOnRoute)
        {
            Status = StatusLoading;
            Touch();
        }
    }

    /// <summary>Sale a ruta: necesita carga y no estar ya en ruta.</summary>
    public void Dispatch(DateTime dispatchedAtUtc)
    {
        if (Status == StatusOnRoute)
        {
            throw new DomainException($"El camión {Name} ya está en ruta.");
        }

        if (stock.Count == 0)
        {
            throw new DomainException($"El camión {Name} no tiene carga; cárguelo antes de despacharlo.");
        }

        Status = StatusOnRoute;
        DispatchedAt = dispatchedAtUtc;
        Touch();
    }

    /// <summary>Cierra la ruta con la liquidación: solo un camión en ruta se liquida.</summary>
    public void MarkSettled()
    {
        if (Status != StatusOnRoute)
        {
            throw new DomainException($"El camión {Name} no está en ruta; solo se liquida un camión que salió a ruta.");
        }

        Status = StatusSettled;
        Touch();
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
