using AccountingApi.Features.CompanyInfo;
using AccountingApi.Infrastructure;
using AccountingApi.Models;
using AccountingApi.Services.CurrentUserService;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.CompanyInfo;

public class DeleteCompanyInfoHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private DeleteCompanyInfoHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _handler = new DeleteCompanyInfoHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object);
    }

    [Test]
    public async Task Handle_DeletesCompanyInfo_WhenValidNonDefaultCompanyWithoutActiveInvoices()
    {
        // Arrange
        var companyId = 1;
        var command = new DeleteCompanyInfoCommand(companyId);
        var currentUser = "test-user";

        var companyInfo = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "Test Company",
            IsDefault = false,
            IsDeleted = false,
            Invoices = new List<Invoice>
            {
                new() { Id = 1, IsDeleted = true }, // Deleted invoice - should not prevent deletion
                new() { Id = 2, IsDeleted = true }  // Deleted invoice - should not prevent deletion
            }
        };

        var companies = new List<AccountingApi.Models.CompanyInfo> { companyInfo };
        var queryableCompanies = companies.AsQueryable();

        var companyInfosDbSetMock = new Mock<DbSet<AccountingApi.Models.CompanyInfo>>();
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.Provider).Returns(queryableCompanies.Provider);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.Expression).Returns(queryableCompanies.Expression);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.ElementType).Returns(queryableCompanies.ElementType);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableCompanies.GetEnumerator());

        _contextMock.Setup(x => x.CompanyInfos).Returns(companyInfosDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(companyInfo.IsDeleted, Is.True);
        Assert.That(companyInfo.DeletedBy, Is.EqualTo(currentUser));
        Assert.That(companyInfo.UpdatedBy, Is.EqualTo(currentUser));
        Assert.That(companyInfo.DeletedAt, Is.Not.Null);
        Assert.That(companyInfo.UpdatedAt, Is.Not.Null);
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _currentUserServiceMock.Verify(x => x.GetCurrentUserForAudit(), Times.Once);
    }

    [Test]
    public async Task Handle_ReturnsFalse_WhenCompanyNotFound()
    {
        // Arrange
        var companyId = 999;
        var command = new DeleteCompanyInfoCommand(companyId);

        var companies = new List<AccountingApi.Models.CompanyInfo>();
        var queryableCompanies = companies.AsQueryable();

        var companyInfosDbSetMock = new Mock<DbSet<AccountingApi.Models.CompanyInfo>>();
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.Provider).Returns(queryableCompanies.Provider);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.Expression).Returns(queryableCompanies.Expression);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.ElementType).Returns(queryableCompanies.ElementType);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableCompanies.GetEnumerator());

        _contextMock.Setup(x => x.CompanyInfos).Returns(companyInfosDbSetMock.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ThrowsException_WhenTryingToDeleteDefaultCompany()
    {
        // Arrange
        var companyId = 1;
        var command = new DeleteCompanyInfoCommand(companyId);

        var companyInfo = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "Default Company",
            IsDefault = true,
            Invoices = new List<Invoice>()
        };

        var companies = new List<AccountingApi.Models.CompanyInfo> { companyInfo };
        var queryableCompanies = companies.AsQueryable();

        var companyInfosDbSetMock = new Mock<DbSet<AccountingApi.Models.CompanyInfo>>();
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.Provider).Returns(queryableCompanies.Provider);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.Expression).Returns(queryableCompanies.Expression);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.ElementType).Returns(queryableCompanies.ElementType);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableCompanies.GetEnumerator());

        _contextMock.Setup(x => x.CompanyInfos).Returns(companyInfosDbSetMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Cannot delete the default company"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ThrowsException_WhenCompanyHasActiveInvoices()
    {
        // Arrange
        var companyId = 1;
        var command = new DeleteCompanyInfoCommand(companyId);

        var companyInfo = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "Test Company",
            IsDefault = false,
            Invoices = new List<Invoice>
            {
                new() { Id = 1, IsDeleted = false }, // Active invoice - should prevent deletion
                new() { Id = 2, IsDeleted = true }   // Deleted invoice - should not prevent deletion
            }
        };

        var companies = new List<AccountingApi.Models.CompanyInfo> { companyInfo };
        var queryableCompanies = companies.AsQueryable();

        var companyInfosDbSetMock = new Mock<DbSet<AccountingApi.Models.CompanyInfo>>();
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.Provider).Returns(queryableCompanies.Provider);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.Expression).Returns(queryableCompanies.Expression);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.ElementType).Returns(queryableCompanies.ElementType);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableCompanies.GetEnumerator());

        _contextMock.Setup(x => x.CompanyInfos).Returns(companyInfosDbSetMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain("Cannot delete company that has active invoices"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_AllowsDeletion_WhenCompanyHasOnlyDeletedInvoices()
    {
        // Arrange
        var companyId = 1;
        var command = new DeleteCompanyInfoCommand(companyId);
        var currentUser = "test-user";

        var companyInfo = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "Test Company",
            IsDefault = false,
            IsDeleted = false,
            Invoices = new List<Invoice>
            {
                new() { Id = 1, IsDeleted = true },
                new() { Id = 2, IsDeleted = true },
                new() { Id = 3, IsDeleted = true }
            }
        };

        var companies = new List<AccountingApi.Models.CompanyInfo> { companyInfo };
        var queryableCompanies = companies.AsQueryable();

        var companyInfosDbSetMock = new Mock<DbSet<AccountingApi.Models.CompanyInfo>>();
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.Provider).Returns(queryableCompanies.Provider);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.Expression).Returns(queryableCompanies.Expression);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.ElementType).Returns(queryableCompanies.ElementType);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableCompanies.GetEnumerator());

        _contextMock.Setup(x => x.CompanyInfos).Returns(companyInfosDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(companyInfo.IsDeleted, Is.True);
        Assert.That(companyInfo.DeletedBy, Is.EqualTo(currentUser));
        Assert.That(companyInfo.UpdatedBy, Is.EqualTo(currentUser));
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_AllowsDeletion_WhenCompanyHasNoInvoices()
    {
        // Arrange
        var companyId = 1;
        var command = new DeleteCompanyInfoCommand(companyId);
        var currentUser = "test-user";

        var companyInfo = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "Test Company",
            IsDefault = false,
            IsDeleted = false,
            Invoices = new List<Invoice>() // Empty list
        };

        var companies = new List<AccountingApi.Models.CompanyInfo> { companyInfo };
        var queryableCompanies = companies.AsQueryable();

        var companyInfosDbSetMock = new Mock<DbSet<AccountingApi.Models.CompanyInfo>>();
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.Provider).Returns(queryableCompanies.Provider);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.Expression).Returns(queryableCompanies.Expression);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.ElementType).Returns(queryableCompanies.ElementType);
        companyInfosDbSetMock.As<IQueryable<AccountingApi.Models.CompanyInfo>>()
            .Setup(m => m.GetEnumerator()).Returns(queryableCompanies.GetEnumerator());

        _contextMock.Setup(x => x.CompanyInfos).Returns(companyInfosDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(companyInfo.IsDeleted, Is.True);
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}