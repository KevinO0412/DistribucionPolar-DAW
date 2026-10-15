using Core.Domain.Common;
using Core.Domain.Entities;

namespace UnitTests.Domain;

internal static class TestData
{
    /// <summary>Polar Pilsen retornable 222 ml x36: $23.00 caja, $0.64 unidad, grupo de vacíos 222ML, autorizado para regalía.</summary>
    public static Product Pilsen(int stockUnits = 3600) => new(
        "POLAR PILSEN RET 222MLx36UN", "Polar Pilsen", "BEB-POL-0021", Guid.NewGuid(),
        23.00m, 0.64m, 18.40m, 36, stockUnits, 180, 7200, true, EmptyReturnGroups.Ret222MlX36, isGiftEligible: true);

    /// <summary>Pepsi 350 ml x24: $11.00 caja, $0.46 unidad.</summary>
    public static Product Pepsi350(int stockUnits = 2400) => new(
        "PEPSI/SABORES 350X24UN", "Pepsi", "BEB-PEP-0040", Guid.NewGuid(),
        11.00m, 0.46m, 8.80m, 24, stockUnits, 120, 4800, true, EmptyReturnGroups.Pepsi350MlX24);

    /// <summary>Producto sin vacíos ni regalía.</summary>
    public static Product Coffee(int stockUnits = 300) => new(
        "BUEN CAFÉ 100X30UN", "Buen Café", "ALI-BUE-0082", Guid.NewGuid(),
        37.50m, 1.25m, 30.00m, 30, stockUnits, 150, 1200, false, null);

    public static Client Client() => new(1, "J-40000001-0", "Bodega La Esquina", null, null, null);

    public static Truck Truck() => new("FVR", "AA200AA");
}
