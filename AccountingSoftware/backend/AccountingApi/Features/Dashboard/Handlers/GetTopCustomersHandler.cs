using MyMediator;
using AccountingApi.Infrastructure;
using AccountingApi.DTOs.Dashboard;
using AccountingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApi.Features.Dashboard.Handlers
{
    public class GetTopCustomersQuery : IRequest<TopCustomersDto>
    {
        public int Limit { get; set; }
        public GetTopCustomersQuery(int limit) => Limit = limit;
    }

    public class GetTopCustomersHandler : IRequestHandler<GetTopCustomersQuery, TopCustomersDto>
    {
        private readonly AccountingDbContext _context;
        public GetTopCustomersHandler(AccountingDbContext context) => _context = context;

        public async Task<TopCustomersDto> Handle(GetTopCustomersQuery request, CancellationToken cancellationToken)
        {
            var topCustomers = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Customer)
                .Where(i => i.Status == InvoiceStatus.Paid)
                .GroupBy(i => i.Customer.CompanyName)
                .Select(g => new { CustomerName = g.Key, Revenue = g.Sum(i => i.TotalAmount) })
                .OrderByDescending(x => x.Revenue)
                .Take(request.Limit)
                .ToListAsync(cancellationToken);

            var colors = new List<string> { "#198754", "#0d6efd", "#ffc107", "#fd7e14", "#6f42c1" };

            return new TopCustomersDto
            {
                Labels = topCustomers.Select(c => c.CustomerName).ToList(),
                Data = topCustomers.Select(c => c.Revenue).ToList(),
                BackgroundColors = colors.Take(topCustomers.Count).ToList()
            };
        }
    }
}
