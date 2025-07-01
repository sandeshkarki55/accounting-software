namespace AccountingApi.Services;

public interface INumberGenerationService
{
    Task<string> GenerateInvoiceNumberAsync();
    Task<string> GenerateCustomerCodeAsync();
}
