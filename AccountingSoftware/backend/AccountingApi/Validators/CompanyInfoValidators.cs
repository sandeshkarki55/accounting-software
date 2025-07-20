using AccountingApi.DTOs;
using AccountingApi.Features.CompanyInfo;

using FluentValidation;

namespace AccountingApi.Validators;

public class CreateCompanyInfoDtoValidator : AbstractValidator<CreateCompanyInfoDto>
{
    public CreateCompanyInfoDtoValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Email).MaximumLength(255).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.TaxNumber).MaximumLength(50);
    }
}

public class UpdateCompanyInfoCommandValidator : AbstractValidator<UpdateCompanyInfoCommand>
{
    public UpdateCompanyInfoCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CompanyInfo).SetValidator(new CreateCompanyInfoDtoValidator());
    }
}

public class SetDefaultCompanyCommandValidator : AbstractValidator<SetDefaultCompanyCommand>
{
    public SetDefaultCompanyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}