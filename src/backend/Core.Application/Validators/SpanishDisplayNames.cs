using System.Reflection;

namespace Core.Application.Validators;

/// <summary>
/// Nombres en español de las propiedades de los DTOs para los mensajes de FluentValidation.
/// </summary>
internal static class SpanishDisplayNames
{
    private static readonly Dictionary<string, string> Names = new()
    {
        ["Name"] = "Nombre",
        ["Description"] = "Descripción",
        ["Deposit"] = "Depósito",
        ["Brand"] = "Marca",
        ["CategoryId"] = "Categoría",
        ["PriceBox"] = "Precio por caja",
        ["PriceUnit"] = "Precio por unidad",
        ["CostPrice"] = "Costo",
        ["UnitsPerBox"] = "Unidades por caja",
        ["InitialBoxes"] = "Cajas iniciales",
        ["InitialLooseUnits"] = "Unidades sueltas iniciales",
        ["MinStock"] = "Stock mínimo",
        ["MaxStock"] = "Stock máximo",
        ["EmptyGroupKey"] = "Grupo de vacíos",
        ["Username"] = "Usuario",
        ["Password"] = "Clave",
        ["Sku"] = "SKU",
        ["CurrentStock"] = "Stock actual",
        ["RawProductName"] = "Nombre del producto",
        ["CategoryName"] = "Nombre de la categoría",
        ["SequenceNumber"] = "Secuencia",
        ["ProductId"] = "Producto",
        ["Boxes"] = "Cajas",
        ["LooseUnits"] = "Unidades sueltas",
        ["Units"] = "Unidades",
        ["GroupKey"] = "Grupo de vacíos",
        ["Groups"] = "Grupos de vacíos",
        ["Mode"] = "Modo de carga",
        ["Observation"] = "Observación",
        ["Destination"] = "Destino",
        ["DestinationTruckId"] = "Camión destino",
        ["Reason"] = "Motivo",
        ["Plate"] = "Placa",
        ["Role"] = "Rol",
        ["Rif"] = "RIF o cédula",
        ["Address"] = "Dirección",
        ["Nickname"] = "Referencia",
        ["Type"] = "Tipo",
        ["Amount"] = "Monto",
        ["MoneyBalance"] = "Saldo de dinero",
        ["ClientId"] = "Cliente",
        ["DispatchOrigin"] = "Origen del despacho",
        ["TruckId"] = "Camión",
        ["EmployeeId"] = "Empleado",
        ["Items"] = "Productos",
        ["Payment"] = "Abono",
        ["PaymentObservation"] = "Observación de pago",
        ["ReturnedEmpties"] = "Vacíos devueltos",
        ["DocumentType"] = "Tipo de documento",
        ["DocumentNumber"] = "Número de documento",
        ["Origin"] = "Origen",
        ["ActionTaken"] = "Acción tomada",
        ["Responsible"] = "Responsable",
        ["Email"] = "Email",
        ["FullName"] = "Nombre completo",
        ["AttendantName"] = "Vendedor asignado",
        ["NewPassword"] = "Nueva clave",
        ["CurrentPassword"] = "Clave actual"
    };

    public static string? Resolve(Type type, MemberInfo? member, System.Linq.Expressions.LambdaExpression? expression)
    {
        return member is not null && Names.TryGetValue(member.Name, out var name) ? name : null;
    }
}
