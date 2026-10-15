using Core.Application.Abstractions;
using Core.Domain.Entities;

namespace Core.Application.Tests;

/// <summary>Mocks y datos de prueba compartidos por las pruebas de servicios.</summary>
internal static class TestDoubles
{
    /// <summary>
    /// Unidad de trabajo simulada: SaveChanges devuelve 1 y ExecuteInTransaction ejecuta la operación recibida,
    /// para que el servicio corra su lógica completa sin base de datos.
    /// </summary>
    public static Mock<IUnitOfWork> UnitOfWork()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        unitOfWork
            .Setup(u => u.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<It.IsAnyType>>>(),
                It.IsAny<CancellationToken>()))
            .Returns(new InvocationFunc(invocation =>
                ((Delegate)invocation.Arguments[0]).DynamicInvoke(invocation.Arguments[1])!));
        return unitOfWork;
    }

    public static Category FoodCategory() => new("Alimentos", null, "Depósito #1 - Alimentos");

    /// <summary>HARINA PAN x20: caja $18.50, unidad $0.95, costo $14.80, mínimo 100 y máximo 800 unidades.</summary>
    public static Product HarinaPan(Guid categoryId, int stockUnits = 45) =>
        new("HARINA PAN 1KGx20UN", "P.A.N.", "ALI-HAR-0001", categoryId, 18.50m, 0.95m, 14.80m, 20, stockUnits, 100, 800, false, null);

    /// <summary>POLAR PILSEN RET 222ML x36 del grupo de vacíos RET_222ML_X36.</summary>
    public static Product PolarPilsen(Guid categoryId, int stockUnits) =>
        new("POLAR PILSEN RET 222MLx36UN", "POLAR", "BEB-POL-0002", categoryId, 23.00m, 0.64m, 18.00m, 36, stockUnits, 72, 3600, true, "RET_222ML_X36");
}
