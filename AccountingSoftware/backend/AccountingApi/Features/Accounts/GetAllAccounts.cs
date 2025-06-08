using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;

namespace AccountingApi.Features.Accounts;

// Query to get all accounts
public record GetAllAccountsQuery : IRequest<List<AccountDto>>;

// Handler for GetAllAccountsQuery
public class GetAllAccountsQueryHandler : IRequestHandler<GetAllAccountsQuery, List<AccountDto>>
{
    private readonly AccountingDbContext _context;
    private readonly AccountMapper _accountMapper;

    public GetAllAccountsQueryHandler(AccountingDbContext context, AccountMapper accountMapper)
    {
        _context = context;
        _accountMapper = accountMapper;
    }

    public async Task<List<AccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _context.Accounts
            .Include(a => a.ParentAccount)
            .OrderBy(a => a.AccountCode)
            .ToListAsync(cancellationToken);

        return _accountMapper.ToDto(accounts).ToList();
    }
}