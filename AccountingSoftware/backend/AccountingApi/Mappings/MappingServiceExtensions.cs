namespace AccountingApi.Mappings;

/// <summary>
/// Extension methods for registering mapping services
/// </summary>
public static class MappingServiceExtensions
{
    /// <summary>
    /// Registers all mapping services with the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMappingServices(this IServiceCollection services)
    {
        // Register individual mappers
        services.AddScoped<AccountMapper>();
        services.AddScoped<CustomerMapper>();
        services.AddScoped<CompanyInfoMapper>();
        services.AddScoped<JournalEntryLineMapper>();
        services.AddScoped<JournalEntryMapper>();
        services.AddScoped<InvoiceItemMapper>();
        services.AddScoped<InvoiceMapper>();

        return services;
    }
}