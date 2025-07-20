using AccountingApi.DTOs;
using AccountingApi.Features.Invoices;

using FluentValidation;

namespace AccountingApi.Validators;

public class CreateInvoiceItemDtoValidator : AbstractValidator<CreateInvoiceItemDto>
{
    public CreateInvoiceItemDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
    }
}

public class CreateInvoiceDtoValidator : AbstractValidator<CreateInvoiceDto>
{
    public CreateInvoiceDtoValidator()
    {
        RuleFor(x => x.InvoiceDate).NotEmpty();
        RuleFor(x => x.DueDate).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.TaxRate).InclusiveBetween(0, 100);
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new CreateInvoiceItemDtoValidator());
    }
}

public class MarkInvoiceAsPaidDtoValidator : AbstractValidator<MarkInvoiceAsPaidDto>
{
    public MarkInvoiceAsPaidDtoValidator()
    {
        RuleFor(x => x.PaidDate).NotEmpty();
    }
}

public class MarkInvoiceAsPaidCommandValidator : AbstractValidator<MarkInvoiceAsPaidCommand>
{
    public MarkInvoiceAsPaidCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.PaidDate).NotEmpty();
    }
}