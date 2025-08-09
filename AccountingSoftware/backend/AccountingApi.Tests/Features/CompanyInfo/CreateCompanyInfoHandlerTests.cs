using AccountingApi.DTOs;
using AccountingApi.Features.CompanyInfo;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;
using AccountingApi.Models;
using AccountingApi.Services.CurrentUserService;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.CompanyInfo;

public class CreateCompanyInfoHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<CompanyInfoMapper> _mapperMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private CreateCompanyInfoCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _mapperMock = new Mock<CompanyInfoMapper>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _handler = new CreateCompanyInfoCommandHandler(
            _contextMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object);
    }

    [Test]
    public async Task Handle_CreatesCompanyInfo_WhenValidRequest()
    {
        // Arrange
        var createCompanyInfoDto = new CreateCompanyInfoDto
        {
            CompanyName = "Test Company",
            LegalName = "Test Company LLC",
            Email = "info@testcompany.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            City = "Anytown",
            State = "Test State",
            PostalCode = "12345",
            Country = "Test Country",
            TaxNumber = "123456789",
            IsDefault = false
        };

        var command = new CreateCompanyInfoCommand(createCompanyInfoDto);
        var currentUser = "test-user";
        
        var companyInfoEntity = new AccountingApi.Models.CompanyInfo
        {
            Id = 1,
            CompanyName = "Test Company",
            LegalName = "Test Company LLC",
            Email = "info@testcompany.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            City = "Anytown",
            State = "Test State",
            PostalCode = "12345",
            Country = "Test Country",
            TaxNumber = "123456789",
            IsDefault = false,
            CreatedBy = currentUser,
            UpdatedBy = currentUser,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var expectedDto = new CompanyInfoDto
        {
            Id = 1,
            CompanyName = "Test Company",
            LegalName = "Test Company LLC",
            Email = "info@testcompany.com",
            Phone = "123-456-7890",
            Address = "123 Main St",
            City = "Anytown",
            State = "Test State",
            PostalCode = "12345",
            Country = "Test Country",
            TaxNumber = "123456789",
            IsDefault = false
        };

        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);
        _mapperMock.Setup(x => x.ToEntity(createCompanyInfoDto)).Returns(companyInfoEntity);
        _mapperMock.Setup(x => x.ToDto(It.IsAny<AccountingApi.Models.CompanyInfo>())).Returns(expectedDto);

        var companyInfosDbSetMock = new Mock<DbSet<AccountingApi.Models.CompanyInfo>>();
        _contextMock.Setup(x => x.CompanyInfos).Returns(companyInfosDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CompanyName, Is.EqualTo("Test Company"));
        Assert.That(result.Email, Is.EqualTo("info@testcompany.com"));
        
        companyInfosDbSetMock.Verify(x => x.Add(It.IsAny<AccountingApi.Models.CompanyInfo>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _currentUserServiceMock.Verify(x => x.GetCurrentUserForAudit(), Times.Once);
    }

    [Test]
    public async Task Handle_UnsetsOtherDefaults_WhenSettingAsDefault()
    {
        // Arrange
        var createCompanyInfoDto = new CreateCompanyInfoDto
        {
            CompanyName = "New Default Company",
            IsDefault = true
        };

        var command = new CreateCompanyInfoCommand(createCompanyInfoDto);
        var currentUser = "test-user";

        var existingDefaultCompanies = new List<AccountingApi.Models.CompanyInfo>
        {
            new() { Id = 1, CompanyName = "Old Default 1", IsDefault = true },
            new() { Id = 2, CompanyName = "Old Default 2", IsDefault = true }
        };

        var newCompanyEntity = new AccountingApi.Models.CompanyInfo
        {
            Id = 3,
            CompanyName = "New Default Company",
            IsDefault = true,
            CreatedBy = currentUser,
            UpdatedBy = currentUser
        };

        var expectedDto = new CompanyInfoDto
        {
            Id = 3,
            CompanyName = "New Default Company",
            IsDefault = true
        };

        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);
        _mapperMock.Setup(x => x.ToEntity(createCompanyInfoDto)).Returns(newCompanyEntity);
        _mapperMock.Setup(x => x.ToDto(It.IsAny<AccountingApi.Models.CompanyInfo>())).Returns(expectedDto);

        var companyInfosDbSetMock = new Mock<DbSet<AccountingApi.Models.CompanyInfo>>();
        var queryableCompanies = existingDefaultCompanies.AsQueryable();
        
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

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDefault, Is.True);
        Assert.That(existingDefaultCompanies.All(c => !c.IsDefault), Is.True);
        Assert.That(existingDefaultCompanies.All(c => c.UpdatedBy == currentUser), Is.True);
        
        companyInfosDbSetMock.Verify(x => x.Add(It.IsAny<AccountingApi.Models.CompanyInfo>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_DoesNotUnsetDefaults_WhenNotSettingAsDefault()
    {
        // Arrange
        var createCompanyInfoDto = new CreateCompanyInfoDto
        {
            CompanyName = "Non-Default Company",
            IsDefault = false
        };

        var command = new CreateCompanyInfoCommand(createCompanyInfoDto);
        var currentUser = "test-user";

        var companyInfoEntity = new AccountingApi.Models.CompanyInfo
        {
            Id = 1,
            CompanyName = "Non-Default Company",
            IsDefault = false,
            CreatedBy = currentUser,
            UpdatedBy = currentUser
        };

        var expectedDto = new CompanyInfoDto
        {
            Id = 1,
            CompanyName = "Non-Default Company",
            IsDefault = false
        };

        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);
        _mapperMock.Setup(x => x.ToEntity(createCompanyInfoDto)).Returns(companyInfoEntity);
        _mapperMock.Setup(x => x.ToDto(It.IsAny<AccountingApi.Models.CompanyInfo>())).Returns(expectedDto);

        var companyInfosDbSetMock = new Mock<DbSet<AccountingApi.Models.CompanyInfo>>();
        _contextMock.Setup(x => x.CompanyInfos).Returns(companyInfosDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDefault, Is.False);
        
        // Verify no query was made to find existing defaults
        companyInfosDbSetMock.Verify(x => x.Add(It.IsAny<AccountingApi.Models.CompanyInfo>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_SetsAuditFields_WhenCreatingCompany()
    {
        // Arrange
        var createCompanyInfoDto = new CreateCompanyInfoDto
        {
            CompanyName = "Audit Test Company"
        };

        var command = new CreateCompanyInfoCommand(createCompanyInfoDto);
        var currentUser = "audit-user";

        var companyInfoEntity = new AccountingApi.Models.CompanyInfo
        {
            CompanyName = "Audit Test Company"
        };

        var expectedDto = new CompanyInfoDto
        {
            CompanyName = "Audit Test Company"
        };

        _currentUserServiceMock.Setup(x => x.GetCurrentUserForAudit()).Returns(currentUser);
        _mapperMock.Setup(x => x.ToEntity(createCompanyInfoDto)).Returns(companyInfoEntity);
        _mapperMock.Setup(x => x.ToDto(It.IsAny<AccountingApi.Models.CompanyInfo>())).Returns(expectedDto);

        var companyInfosDbSetMock = new Mock<DbSet<AccountingApi.Models.CompanyInfo>>();
        _contextMock.Setup(x => x.CompanyInfos).Returns(companyInfosDbSetMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(companyInfoEntity.CreatedBy, Is.EqualTo(currentUser));
        Assert.That(companyInfoEntity.UpdatedBy, Is.EqualTo(currentUser));
        
        _currentUserServiceMock.Verify(x => x.GetCurrentUserForAudit(), Times.Once);
    }
}