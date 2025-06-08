using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;

namespace AccountingApi.Features.Accounts;

// Command to update an existing account
public record UpdateAccountCommand(int Id, UpdateAccountDto Account) : IRequest<bool>;

// Handler for UpdateAccountCommand
public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, bool>
{
    private readonly AccountingDbContext _context;

    public UpdateAccountCommandHandler(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (account == null)
            return false;

        // Update account properties
        account.AccountName = request.Account.AccountName;
        account.Description = request.Account.Description;
        account.IsActive = request.Account.IsActive;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}