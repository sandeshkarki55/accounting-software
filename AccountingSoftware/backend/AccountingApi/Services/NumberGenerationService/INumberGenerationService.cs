namespace AccountingApi.Services.NumberGenerationService;

public interface INumberGenerationService
{
    Task<string> GenerateInvoiceNumberAsync();

    Task<string> GenerateCustomerCodeAsync();
}