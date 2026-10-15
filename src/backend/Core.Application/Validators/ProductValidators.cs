using Core.Application.DTOs;
using Core.Domain.Common;
using Core.Domain.Entities;
using FluentValidation;

namespace Core.Application.Validators;

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Product.NameMaxLength);
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(Product.BrandMaxLength);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.PriceBox).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.PriceUnit).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.UnitsPerBox).GreaterThan(0);
        RuleFor(x => x.InitialBoxes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.InitialLooseUnits).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxStock)
            .GreaterThan(x => x.MinStock)
            .WithMessage("El stock máximo debe ser mayor que el stock mínimo.");
        RuleFor(x => x.EmptyGroupKey)
            .Must(key => key is null || EmptyReturnGroups.Exists(key))
            .WithMessage($"El grupo de vacíos no existe. Valores permitidos: {string.Join(", ", EmptyReturnGroups.All.Keys)}.");
    }
}

public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Product.NameMaxLength);
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(Product.BrandMaxLength);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.PriceBox).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.PriceUnit).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.UnitsPerBox).GreaterThan(0);
        RuleFor(x => x.MinStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxStock)
            .GreaterThan(x => x.MinStock)
            .WithMessage("El stock máximo debe ser mayor que el stock mínimo.");
        RuleFor(x => x.EmptyGroupKey)
            .Must(key => key is null || EmptyReturnGroups.Exists(key))
            .WithMessage($"El grupo de vacíos no existe. Valores permitidos: {string.Join(", ", EmptyReturnGroups.All.Keys)}.");
    }
}
