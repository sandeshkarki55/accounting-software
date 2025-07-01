using Microsoft.EntityFrameworkCore;
using AccountingApi.Infrastructure;

namespace AccountingApi.Services;

public class NumberGenerationService : INumberGenerationService
{
    private readonly AccountingDbContext _context;

    public NumberGenerationService(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        // Use a stored procedure or direct SQL execution to get the next sequence value
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT NEXT VALUE FOR InvoiceSequence";
        
        await _context.Database.OpenConnectionAsync();
        try
        {
            var result = await command.ExecuteScalarAsync();
            var nextValue = Convert.ToInt32(result);
            return $"INV-{nextValue}";
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<string> GenerateCustomerCodeAsync()
    {
        // Use a stored procedure or direct SQL execution to get the next sequence value
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT NEXT VALUE FOR CustomerSequence";
        
        await _context.Database.OpenConnectionAsync();
        try
        {
            var result = await command.ExecuteScalarAsync();
            var nextValue = Convert.ToInt32(result);
            return $"CUST-{nextValue}";
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }
}
