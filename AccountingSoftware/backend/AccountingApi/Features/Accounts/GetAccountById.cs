using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;

namespace AccountingApi.Features.Accounts;

// Query to get account by ID
using MyMediator;
public record GetAccountByIdQuery(int Id) : IRequest<AccountDto?>;

// Handler for GetAccountByIdQuery
public class GetAccountByIdQueryHandler(AccountingDbContext context, AccountMapper accountMapper) : IRequestHandler<GetAccountByIdQuery, AccountDto?>
{
    public async Task<AccountDto?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await context.Accounts
            .Include(a => a.ParentAccount)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (account == null)
            return null;

        return accountMapper.ToDto(account);
    }
}