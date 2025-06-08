using FluentValidation;
using AccountingApi.DTOs;
using AccountingApi.Models;

namespace AccountingApi.Validators;

public class CreateAccountDtoValidator : AbstractValidator<CreateAccountDto>
{
    public CreateAccountDtoValidator()
    {
        RuleFor(x => x.AccountCode)
            .NotEmpty().WithMessage("Account code is required")
            .Length(1, 20).WithMessage("Account code must be between 1 and 20 characters")
            .Matches("^[A-Z0-9-]+$").WithMessage("Account code can only contain uppercase letters, numbers, and hyphens");

        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("Account name is required")
            .Length(1, 200).WithMessage("Account name must be between 1 and 200 characters");

        RuleFor(x => x.AccountType)
            .IsInEnum().WithMessage("Invalid account type");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.ParentAccountId)
            .GreaterThan(0).When(x => x.ParentAccountId.HasValue)
            .WithMessage("Parent account ID must be greater than 0");
    }
}

public class UpdateAccountDtoValidator : AbstractValidator<UpdateAccountDto>
{
    public UpdateAccountDtoValidator()
    {
        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("Account name is required")
            .Length(1, 200).WithMessage("Account name must be between 1 and 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
    }
}