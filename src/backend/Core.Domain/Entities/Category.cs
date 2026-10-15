using Core.Domain.Common;

namespace Core.Domain.Entities;

/// <summary>
/// Categoría del catálogo (Alimentos, Bebidas, Jabones/P&amp;G). Cada categoría se almacena en un depósito del galpón.
/// </summary>
public sealed class Category : BaseEntity
{
    public const int NameMaxLength = 50;
    public const int DescriptionMaxLength = 250;
    public const int DepositMaxLength = 100;

    private readonly List<Product> products = [];

    // Constructor para EF Core.
    private Category()
    {
    }

    public Category(string name, string? description, string deposit)
    {
        Apply(name, description, deposit);
    }

    public string Name { get; private set; } = default!;

    public string? Description { get; private set; }

    public string Deposit { get; private set; } = default!;

    public IReadOnlyCollection<Product> Products => products.AsReadOnly();

    public void Update(string name, string? description, string deposit)
    {
        Apply(name, description, deposit);
        Touch();
    }

    private void Apply(string name, string? description, string deposit)
    {
        Name = Guard.RequiredText(name, "nombre de la categoría", NameMaxLength);
        Description = Guard.OptionalText(description, "descripción de la categoría", DescriptionMaxLength);
        Deposit = Guard.RequiredText(deposit, "depósito", DepositMaxLength);
    }
}
