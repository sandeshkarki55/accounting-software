using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;

namespace AccountingApi.Features.Accounts;

// Command to update an existing account
public record UpdateAccountCommand(int Id, UpdateAccountDto Account) : IRequest<bool>;

// Handler for UpdateAccountCommand
public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, bool>
{
    private readonly AccountingDbContext _context;
    private readonly AccountMapper _accountMapper;

    public UpdateAccountCommandHandler(AccountingDbContext context, AccountMapper accountMapper)
    {
        _context = context;
        _accountMapper = accountMapper;
    }

    public async Task<bool> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (account == null)
            return false;

        // Update account using mapper
        _accountMapper.UpdateEntity(account, request.Account);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}