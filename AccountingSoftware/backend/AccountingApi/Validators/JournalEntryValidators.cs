using FluentValidation;
using AccountingApi.DTOs;

namespace AccountingApi.Validators;

public class CreateJournalEntryLineDtoValidator : AbstractValidator<CreateJournalEntryLineDto>
{
    public CreateJournalEntryLineDtoValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DebitAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CreditAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x).Must(x => x.DebitAmount > 0 || x.CreditAmount > 0)
            .WithMessage("Either DebitAmount or CreditAmount must be greater than 0.");
        RuleFor(x => x).Must(x => x.DebitAmount == 0 || x.CreditAmount == 0)
            .WithMessage("DebitAmount and CreditAmount cannot both have values.");
    }
}

public class CreateJournalEntryDtoValidator : AbstractValidator<CreateJournalEntryDto>
{
    public CreateJournalEntryDtoValidator()
    {
        RuleFor(x => x.TransactionDate).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new CreateJournalEntryLineDtoValidator());
        RuleFor(x => x.Lines).Must(lines =>
        {
            var totalDebits = lines.Sum(l => l.DebitAmount);
            var totalCredits = lines.Sum(l => l.CreditAmount);
            return totalDebits == totalCredits;
        }).WithMessage("Debits and credits must be balanced.");
    }
}

public class UpdateJournalEntryDtoValidator : AbstractValidator<UpdateJournalEntryDto>
{
    public UpdateJournalEntryDtoValidator()
    {
        RuleFor(x => x.TransactionDate).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new CreateJournalEntryLineDtoValidator());
        RuleFor(x => x.Lines).Must(lines =>
        {
            var totalDebits = lines.Sum(l => l.DebitAmount);
            var totalCredits = lines.Sum(l => l.CreditAmount);
            return totalDebits == totalCredits;
        }).WithMessage("Debits and credits must be balanced.");
    }
}
