using Core.Application.DTOs;
using FluentValidation;

namespace Core.Application.Validators;

public sealed class HealthCalculatorRequestValidator : AbstractValidator<HealthCalculatorRequest>
{
    public HealthCalculatorRequestValidator()
    {
        RuleFor(x => x.MinStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxStock)
            .GreaterThan(x => x.MinStock)
            .WithMessage("El stock máximo debe ser mayor que el stock mínimo.");
    }
}

public sealed class SkuGeneratorRequestValidator : AbstractValidator<SkuGeneratorRequest>
{
    public SkuGeneratorRequestValidator()
    {
        RuleFor(x => x.RawProductName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SequenceNumber).InclusiveBetween(1, 9999);
    }
}
