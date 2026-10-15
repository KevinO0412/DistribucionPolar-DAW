namespace Core.Domain.Common;

/// <summary>
/// Grupos de envases retornables (vacíos). Solo los productos asociados a un grupo generan vacíos al venderse.
/// </summary>
public static class EmptyReturnGroups
{
    public const string Ret222MlX36 = "RET_222ML_X36";
    public const string RetPolarPilsen330MlX24 = "RET_POLAR_PILSEN_330ML_X24";
    public const string Pepsi125LX6 = "PEPSI_125L_X6";
    public const string Pepsi350MlX24 = "PEPSI_350ML_X24";

    private static readonly Dictionary<string, string> Names = new()
    {
        [Ret222MlX36] = "222ML RET x36",
        [RetPolarPilsen330MlX24] = "Polar Pilsen 330ML RET x24",
        [Pepsi125LX6] = "Pepsi/Sabores 1.25L x6",
        [Pepsi350MlX24] = "Pepsi/Sabores 350ML x24"
    };

    public static IReadOnlyDictionary<string, string> All => Names;

    public static bool Exists(string groupKey) => Names.ContainsKey(groupKey);

    public static string GetName(string groupKey) =>
        Names.TryGetValue(groupKey, out var name) ? name : "Vacío retornable";
}
