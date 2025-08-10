using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using AccountingApi.Models;
using AccountingApi.Services.CurrentUserService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AccountingApi.Tests.TestHelpers;

/// <summary>
/// Base test class that provides an in-memory database setup for testing
/// </summary>
public abstract class BaseTestWithInMemoryDb
{
    protected AccountingDbContext Context { get; private set; } = null!;
    protected Mock<ICurrentUserService> CurrentUserServiceMock { get; private set; } = null!;
    
    // Real mapper instances instead of mocks
    protected AccountMapper AccountMapper { get; private set; } = null!;
    protected JournalEntryLineMapper JournalEntryLineMapper { get; private set; } = null!;
    protected JournalEntryMapper JournalEntryMapper { get; private set; } = null!;
    protected CompanyInfoMapper CompanyInfoMapper { get; private set; } = null!;
    protected CustomerMapper CustomerMapper { get; private set; } = null!;
    protected InvoiceItemMapper InvoiceItemMapper { get; private set; } = null!;
    protected InvoiceMapper InvoiceMapper { get; private set; } = null!;

    [SetUp]
    public virtual void SetUp()
    {
        // Create in-memory database with unique name for each test
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new AccountingDbContext(options);

        // Ensure database is created
        Context.Database.EnsureCreated();

        // Setup current user service mock
        CurrentUserServiceMock = new Mock<ICurrentUserService>();
        CurrentUserServiceMock.Setup(x => x.UserId).Returns("test-user-id");
        CurrentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns("test-user-id");

        // Create real mapper instances
        SetupMappers();

        // Seed test data if needed
        SeedTestData();
    }

    [TearDown]
    public virtual void TearDown()
    {
        Context?.Dispose();
    }

    /// <summary>
    /// Setup real mapper instances instead of mocking them
    /// </summary>
    private void SetupMappers()
    {
        AccountMapper = new AccountMapper();
        JournalEntryLineMapper = new JournalEntryLineMapper();
        JournalEntryMapper = new JournalEntryMapper(JournalEntryLineMapper);
        CompanyInfoMapper = new CompanyInfoMapper();
        CustomerMapper = new CustomerMapper();
        InvoiceItemMapper = new InvoiceItemMapper();
        InvoiceMapper = new InvoiceMapper(InvoiceItemMapper);
    }

    /// <summary>
    /// Override this method in derived classes to seed specific test data
    /// </summary>
    protected virtual void SeedTestData()
    {
        // Default seeding - can be overridden in derived classes
    }

    /// <summary>
    /// Helper method to add test accounts to the database
    /// </summary>
    protected void AddTestAccounts()
    {
        var accounts = new List<Account>
        {
            new()
            {
                Id = 1,
                AccountCode = "1000",
                AccountName = "Cash",
                AccountType = AccountType.Asset,
                IsActive = true,
                CreatedBy = "test-user",
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = "test-user",
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                AccountCode = "3000",
                AccountName = "Revenue",
                AccountType = AccountType.Revenue,
                IsActive = true,
                CreatedBy = "test-user",
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = "test-user",
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 3,
                AccountCode = "4000",
                AccountName = "Expenses",
                AccountType = AccountType.Expense,
                IsActive = true,
                CreatedBy = "test-user",
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = "test-user",
                UpdatedAt = DateTime.UtcNow
            }
        };

        Context.Accounts.AddRange(accounts);
        Context.SaveChanges();
    }

    /// <summary>
    /// Helper method to add test companies to the database
    /// </summary>
    protected void AddTestCompanies()
    {
        var companies = new List<CompanyInfo>
        {
            new()
            {
                Id = 1,
                CompanyName = "Test Company 1",
                Address = "123 Test St",
                Phone = "123-456-7890",
                Email = "test1@company.com",
                IsDefault = true,
                CreatedBy = "test-user",
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = "test-user",
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                CompanyName = "Test Company 2",
                Address = "456 Test Ave",
                Phone = "987-654-3210",
                Email = "test2@company.com",
                IsDefault = false,
                CreatedBy = "test-user",
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = "test-user",
                UpdatedAt = DateTime.UtcNow
            }
        };

        Context.CompanyInfos.AddRange(companies);
        Context.SaveChanges();
    }

    /// <summary>
    /// Helper method to add test customers to the database
    /// </summary>
    protected void AddTestCustomers()
    {
        var customers = new List<Customer>
        {
            new()
            {
                Id = 1,
                CustomerCode = "CUST001",
                CompanyName = "Test Customer 1",
                Email = "customer1@test.com",
                Phone = "111-222-3333",
                Address = "123 Customer St",
                CreatedBy = "test-user",
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = "test-user",
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                CustomerCode = "CUST002",
                CompanyName = "Test Customer 2",
                Email = "customer2@test.com",
                Phone = "444-555-6666",
                Address = "456 Customer Ave",
                CreatedBy = "test-user",
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = "test-user",
                UpdatedAt = DateTime.UtcNow
            }
        };

        Context.Customers.AddRange(customers);
        Context.SaveChanges();
    }
}