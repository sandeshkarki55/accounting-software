using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;

namespace AccountingApi.Features.Accounts;

// Query to get all accounts
public record GetAllAccountsQuery : IRequest<List<AccountDto>>;

// Handler for GetAllAccountsQuery
public class GetAllAccountsQueryHandler(AccountingDbContext context, AccountMapper accountMapper) : IRequestHandler<GetAllAccountsQuery, List<AccountDto>>
{
    public async Task<List<AccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await context.Accounts
            .Include(a => a.ParentAccount)
            .OrderBy(a => a.AccountCode)
            .ToListAsync(cancellationToken);

        return accountMapper.ToDto(accounts).ToList();
    }
}