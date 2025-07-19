using MyMediator;
using AccountingApi.Infrastructure;
using AccountingApi.DTOs.Dashboard;
using AccountingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApi.Features.Dashboard.Handlers
{
    public class GetPaymentTrendQuery : IRequest<PaymentTrendDto>
    {
        public int Months { get; set; }
        public GetPaymentTrendQuery(int months) => Months = months;
    }

    public class GetPaymentTrendHandler : IRequestHandler<GetPaymentTrendQuery, PaymentTrendDto>
    {
        private readonly AccountingDbContext _context;
        public GetPaymentTrendHandler(AccountingDbContext context) => _context = context;

        public async Task<PaymentTrendDto> Handle(GetPaymentTrendQuery request, CancellationToken cancellationToken)
        {
            var labels = new List<string>();
            var data = new List<decimal>();
            var currentDate = DateTime.Now;

            for (int i = request.Months - 1; i >= 0; i--)
            {
                var date = currentDate.AddMonths(-i);
                labels.Add(date.ToString("MMM yy"));

                var monthPayments = await _context.Invoices
                    .AsNoTracking()
                    .Where(i => i.Status == InvoiceStatus.Paid &&
                               i.InvoiceDate.Month == date.Month &&
                               i.InvoiceDate.Year == date.Year)
                    .SumAsync(i => i.TotalAmount, cancellationToken);

                data.Add(monthPayments);
            }

            return new PaymentTrendDto
            {
                Labels = labels,
                Data = data
            };
        }
    }
}
