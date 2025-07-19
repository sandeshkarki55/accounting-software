using MyMediator;
using AccountingApi.Infrastructure;
using AccountingApi.DTOs.Dashboard;
using AccountingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApi.Features.Dashboard.Handlers
{
    public class GetAccountBalanceOverviewQuery : IRequest<AccountBalanceOverviewDto> { }

    public class GetAccountBalanceOverviewHandler : IRequestHandler<GetAccountBalanceOverviewQuery, AccountBalanceOverviewDto>
    {
        private readonly AccountingDbContext _context;
        public GetAccountBalanceOverviewHandler(AccountingDbContext context) => _context = context;

        public async Task<AccountBalanceOverviewDto> Handle(GetAccountBalanceOverviewQuery request, CancellationToken cancellationToken)
        {
            var accounts = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.IsActive)
                .ToListAsync(cancellationToken);

            var totalAssets = accounts.Where(a => a.AccountType == AccountType.Asset).Sum(a => a.Balance);
            var totalLiabilities = accounts.Where(a => a.AccountType == AccountType.Liability).Sum(a => a.Balance);
            var equity = totalAssets - totalLiabilities;

            var labels = new List<string> { "Assets", "Liabilities", "Equity" };
            var data = new List<decimal> { totalAssets, totalLiabilities, equity };
            var colors = new List<string> { "#28a745", "#dc3545", "#007bff" };

            return new AccountBalanceOverviewDto
            {
                Labels = labels,
                Data = data,
                BackgroundColors = colors
            };
        }
    }
}
