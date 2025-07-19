using MyMediator;
using AccountingApi.Infrastructure;
using AccountingApi.DTOs.Dashboard;
using AccountingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApi.Features.Dashboard.Handlers
{
    public class GetInvoiceStatusDistributionQuery : IRequest<InvoiceStatusDistributionDto> { }

    public class GetInvoiceStatusDistributionHandler : IRequestHandler<GetInvoiceStatusDistributionQuery, InvoiceStatusDistributionDto>
    {
        private readonly AccountingDbContext _context;
        public GetInvoiceStatusDistributionHandler(AccountingDbContext context) => _context = context;

        public async Task<InvoiceStatusDistributionDto> Handle(GetInvoiceStatusDistributionQuery request, CancellationToken cancellationToken)
        {
            var statusCounts = await _context.Invoices
                .AsNoTracking()
                .GroupBy(i => i.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var labels = new List<string>();
            var data = new List<int>();
            var colors = new List<string> { "#6c757d", "#0d6efd", "#198754", "#dc3545", "#343a40", "#ffc107" };

            var allStatuses = Enum.GetValues<InvoiceStatus>();
            var statusLabels = new Dictionary<InvoiceStatus, string>
            {
                { InvoiceStatus.Draft, "Draft" },
                { InvoiceStatus.Sent, "Sent" },
                { InvoiceStatus.Paid, "Paid" },
                { InvoiceStatus.Overdue, "Overdue" },
                { InvoiceStatus.Cancelled, "Cancelled" },
                { InvoiceStatus.PartiallyPaid, "Partially Paid" }
            };

            foreach (var status in allStatuses)
            {
                var count = statusCounts.FirstOrDefault(sc => sc.Status == status)?.Count ?? 0;
                labels.Add(statusLabels[status]);
                data.Add(count);
            }

            return new InvoiceStatusDistributionDto
            {
                Labels = labels,
                Data = data,
                BackgroundColors = colors
            };
        }
    }
}
