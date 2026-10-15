namespace Infrastructure.Persistence.Seed;

/// <summary>
/// Datos iniciales de operación (ficticios): empleados, camiones y clientes de ejemplo.
/// UUID fijos para que las migraciones sean deterministas.
/// </summary>
internal static class OperationsSeedData
{
    private static readonly DateTime SeedDate = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

    private static Guid Id(int group, int n) => Guid.Parse($"{group:D8}-0000-4000-8000-{n:D12}");

    public static object[] Employees { get; } =
    [
        Employee(1, "Carlos Pérez", "Vendedor"),
        Employee(2, "María González", "Vendedora"),
        Employee(3, "Luis Hernández", "Vendedor"),
        Employee(4, "José Rodríguez", "Despachador"),
        Employee(5, "Pedro Ramírez", "Chofer"),
        Employee(6, "Ana Martínez", "Administración"),
        Employee(7, "Laura Torres", "Vendedora")
    ];

    public static object[] Trucks { get; } =
    [
        Truck(1, "Cargo 815", "AA100AA", 600),
        Truck(2, "FVR", "AA200AA", 500),
        Truck(3, "Mack", "AA300AA", 900),
        Truck(4, "Kodiak", "AA400AA", 700),
        Truck(5, "F-350", "AA500AA", 150),
        Truck(6, "Montana", "AA600AA", 60)
    ];

    public static object[] Clients { get; } =
    [
        Client(1, "J-40000001-0", "Bodega La Esquina", "Av. Principal, local 3", "La Esquina", "Ocasional"),
        Client(2, "J-40000002-0", "Abasto El Progreso", "Calle 5 con carrera 7", "El Progreso", "ESPECIAL"),
        Client(3, "J-40000003-0", "Licorería Los Andes", "Av. Los Andes, local 12", "Los Andes", "ESPECIAL"),
        Client(4, "V-10000004", "Panadería Santa Rosa", "Barrio Santa Rosa, calle 2", "Santa Rosa", "Ocasional"),
        Client(5, "J-40000005-0", "Restaurante El Fogón", "Carrera 10, centro", "El Fogón", "Ocasional")
    ];

    public static Guid EmployeeId(int n) => Id(100, n);

    public static Guid TruckId(int n) => Id(200, n);

    public static Guid ClientId(int n) => Id(300, n);

    private static object Employee(int n, string name, string role) => new
    {
        Id = EmployeeId(n),
        Name = name,
        Role = role,
        IsActive = true,
        CreatedAt = SeedDate,
        IsDeleted = false
    };

    private static object Truck(int n, string name, string plate, int capacityBoxes) => new
    {
        Id = TruckId(n),
        Name = name,
        Plate = plate,
        CapacityBoxes = capacityBoxes,
        Status = Core.Domain.Entities.Truck.DefaultStatus,
        CreatedAt = SeedDate,
        IsDeleted = false
    };

    private static object Client(int n, string rif, string name, string address, string nickname, string type) => new
    {
        Id = ClientId(n),
        Code = n,
        Rif = rif,
        Name = name,
        Address = address,
        Nickname = nickname,
        Type = type,
        MoneyBalance = 0m,
        EmptyBoxesBalance = 0,
        EmptyUnitsBalance = 0,
        CreditDays = 30,
        CreatedAt = SeedDate,
        IsDeleted = false
    };
}
