using Core.Domain.Entities;
using Core.Domain.Exceptions;

namespace UnitTests.Domain;

/// <summary>Estados del camión (observación 4): En galpón → Cargando → En ruta → Liquidado → En galpón.</summary>
public sealed class TruckStatusTests
{
    private static readonly DateTime Now = new(2026, 10, 15, 13, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void FullCycle_FollowsTheRouteStates()
    {
        var truck = new Truck("NPR", "AB123CD");
        var product = Guid.NewGuid();

        truck.AddStock(product, 36);
        truck.MarkLoading();
        Assert.Equal(Truck.StatusLoading, truck.Status);

        truck.Dispatch(Now);
        Assert.True(truck.IsOnRoute);
        Assert.Equal(Now, truck.DispatchedAt);

        truck.MarkLoading(); // reabastecimiento en ruta: sigue en ruta
        Assert.Equal(Truck.StatusOnRoute, truck.Status);

        truck.MarkSettled();
        Assert.Equal(Truck.StatusSettled, truck.Status);

        truck.Unload();
        Assert.Equal(Truck.StatusInWarehouse, truck.Status);
    }

    [Fact]
    public void Dispatch_TwiceOrEmpty_IsRejected()
    {
        var truck = new Truck("NPR", "AB123CD");
        Assert.Throws<DomainException>(() => truck.Dispatch(Now));

        truck.AddStock(Guid.NewGuid(), 10);
        truck.Dispatch(Now);
        Assert.Throws<DomainException>(() => truck.Dispatch(Now));
    }

    [Fact]
    public void Settle_OnlyFromOnRoute()
    {
        var truck = new Truck("NPR", "AB123CD");
        Assert.Throws<DomainException>(truck.MarkSettled);
    }
}
