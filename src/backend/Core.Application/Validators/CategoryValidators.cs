using Core.Application.DTOs;
using Core.Domain.Entities;
using FluentValidation;

namespace Core.Application.Validators;

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Category.NameMaxLength);
        RuleFor(x => x.Description).MaximumLength(Category.DescriptionMaxLength);
        RuleFor(x => x.Deposit).NotEmpty().MaximumLength(Category.DepositMaxLength);
    }
}

public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Category.NameMaxLength);
        RuleFor(x => x.Description).MaximumLength(Category.DescriptionMaxLength);
        RuleFor(x => x.Deposit).NotEmpty().MaximumLength(Category.DepositMaxLength);
    }
}
