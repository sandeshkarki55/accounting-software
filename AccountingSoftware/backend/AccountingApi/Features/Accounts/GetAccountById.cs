using MediatR;
using Microsoft.EntityFrameworkCore;
using AccountingApi.DTOs;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;

namespace AccountingApi.Features.Accounts;

// Query to get account by ID
public record GetAccountByIdQuery(int Id) : IRequest<AccountDto?>;

// Handler for GetAccountByIdQuery
public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto?>
{
    private readonly AccountingDbContext _context;
    private readonly AccountMapper _accountMapper;

    public GetAccountByIdQueryHandler(AccountingDbContext context, AccountMapper accountMapper)
    {
        _context = context;
        _accountMapper = accountMapper;
    }

    public async Task<AccountDto?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .Include(a => a.ParentAccount)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (account == null)
            return null;

        return _accountMapper.ToDto(account);
    }
}