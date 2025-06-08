using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;

namespace AccountingApi.Features.Accounts;

// Query to get all accounts
public record GetAllAccountsQuery : IRequest<List<AccountDto>>;

// Handler for GetAllAccountsQuery
public class GetAllAccountsQueryHandler : IRequestHandler<GetAllAccountsQuery, List<AccountDto>>
{
    private readonly AccountingDbContext _context;

    public GetAllAccountsQueryHandler(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<List<AccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _context.Accounts
            .Include(a => a.ParentAccount)
            .OrderBy(a => a.AccountCode)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                AccountType = a.AccountType,
                Balance = a.Balance,
                IsActive = a.IsActive,
                Description = a.Description,
                ParentAccountId = a.ParentAccountId,
                ParentAccountName = a.ParentAccount != null ? a.ParentAccount.AccountName : null
            })
            .ToListAsync(cancellationToken);

        return accounts;
    }
}