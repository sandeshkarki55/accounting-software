using AccountingApi.DTOs;
using AccountingApi.Features.CompanyInfo;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.CompanyInfo;

public class SetDefaultCompanyHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<CompanyInfoMapper> _mapperMock = null!;
    private SetDefaultCompanyHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _mapperMock = new Mock<CompanyInfoMapper>();
        
        _handler = new SetDefaultCompanyHandler(
            _contextMock.Object,
            _mapperMock.Object);
    }

    [Test]
    public async Task Handle_SetsCompanyAsDefault_WhenValidCompanyExists()
    {
        // Arrange
        var companyId = 1;
        var command = new SetDefaultCompanyCommand(companyId);

        var targetCompany = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "Target Company",
            IsDefault = false
        };

        var otherDefaultCompanies = new List<AccountingApi.Models.CompanyInfo>
        {
            new() { Id = 2, CompanyName = "Old Default 1", IsDefault = true },
            new() { Id = 3, CompanyName = "Old Default 2", IsDefault = true }
        };

        var allCompanies = new List<AccountingApi.Models.CompanyInfo> { targetCompany }
            .Concat(otherDefaultCompanies).ToList();

        var expectedDto = new CompanyInfoDto
        {
            Id = companyId,
            CompanyName = "Target Company",
            IsDefault = true
        };

        var queryableCompanies = allCompanies.AsQueryable();
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
        _mapperMock.Setup(x => x.ToDto(targetCompany)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(companyId));
        Assert.That(result.IsDefault, Is.True);
        Assert.That(targetCompany.IsDefault, Is.True);
        Assert.That(otherDefaultCompanies.All(c => !c.IsDefault), Is.True);
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(x => x.ToDto(targetCompany), Times.Once);
    }

    [Test]
    public async Task Handle_ThrowsKeyNotFoundException_WhenCompanyNotFound()
    {
        // Arrange
        var companyId = 999;
        var command = new SetDefaultCompanyCommand(companyId);

        var companies = new List<AccountingApi.Models.CompanyInfo>
        {
            new() { Id = 1, CompanyName = "Company 1", IsDefault = false },
            new() { Id = 2, CompanyName = "Company 2", IsDefault = true }
        };

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
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.That(ex.Message, Does.Contain($"Company with ID {companyId} not found"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_SetsAlreadyDefaultCompany_WithoutChangingOthers()
    {
        // Arrange
        var companyId = 1;
        var command = new SetDefaultCompanyCommand(companyId);

        var targetCompany = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "Already Default Company",
            IsDefault = true
        };

        var otherCompanies = new List<AccountingApi.Models.CompanyInfo>
        {
            new() { Id = 2, CompanyName = "Non-Default Company", IsDefault = false }
        };

        var allCompanies = new List<AccountingApi.Models.CompanyInfo> { targetCompany }
            .Concat(otherCompanies).ToList();

        var expectedDto = new CompanyInfoDto
        {
            Id = companyId,
            CompanyName = "Already Default Company",
            IsDefault = true
        };

        var queryableCompanies = allCompanies.AsQueryable();
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
        _mapperMock.Setup(x => x.ToDto(targetCompany)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDefault, Is.True);
        Assert.That(targetCompany.IsDefault, Is.True);
        Assert.That(otherCompanies.All(c => !c.IsDefault), Is.True);
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_UnsetsOnlyOtherDefaultCompanies_WhenSettingNewDefault()
    {
        // Arrange
        var companyId = 3;
        var command = new SetDefaultCompanyCommand(companyId);

        var targetCompany = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "New Default Company",
            IsDefault = false
        };

        var currentDefaultCompanies = new List<AccountingApi.Models.CompanyInfo>
        {
            new() { Id = 1, CompanyName = "Old Default 1", IsDefault = true },
            new() { Id = 2, CompanyName = "Old Default 2", IsDefault = true }
        };

        var nonDefaultCompanies = new List<AccountingApi.Models.CompanyInfo>
        {
            new() { Id = 4, CompanyName = "Non-Default Company", IsDefault = false }
        };

        var allCompanies = new List<AccountingApi.Models.CompanyInfo> { targetCompany }
            .Concat(currentDefaultCompanies)
            .Concat(nonDefaultCompanies)
            .ToList();

        var expectedDto = new CompanyInfoDto
        {
            Id = companyId,
            CompanyName = "New Default Company",
            IsDefault = true
        };

        var queryableCompanies = allCompanies.AsQueryable();
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
        _mapperMock.Setup(x => x.ToDto(targetCompany)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDefault, Is.True);
        Assert.That(targetCompany.IsDefault, Is.True);
        Assert.That(currentDefaultCompanies.All(c => !c.IsDefault), Is.True);
        Assert.That(nonDefaultCompanies.All(c => !c.IsDefault), Is.True);
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_HandlesEmptyOtherDefaultsList_WhenNoOtherDefaultsExist()
    {
        // Arrange
        var companyId = 1;
        var command = new SetDefaultCompanyCommand(companyId);

        var targetCompany = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "Only Company",
            IsDefault = false
        };

        var allCompanies = new List<AccountingApi.Models.CompanyInfo> { targetCompany };

        var expectedDto = new CompanyInfoDto
        {
            Id = companyId,
            CompanyName = "Only Company",
            IsDefault = true
        };

        var queryableCompanies = allCompanies.AsQueryable();
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
        _mapperMock.Setup(x => x.ToDto(targetCompany)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDefault, Is.True);
        Assert.That(targetCompany.IsDefault, Is.True);
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_DoesNotAffectNonDefaultCompanies_WhenSettingDefault()
    {
        // Arrange
        var companyId = 1;
        var command = new SetDefaultCompanyCommand(companyId);

        var targetCompany = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "Target Company",
            IsDefault = false
        };

        var nonDefaultCompanies = new List<AccountingApi.Models.CompanyInfo>
        {
            new() { Id = 2, CompanyName = "Non-Default 1", IsDefault = false },
            new() { Id = 3, CompanyName = "Non-Default 2", IsDefault = false }
        };

        var allCompanies = new List<AccountingApi.Models.CompanyInfo> { targetCompany }
            .Concat(nonDefaultCompanies).ToList();

        var expectedDto = new CompanyInfoDto
        {
            Id = companyId,
            CompanyName = "Target Company",
            IsDefault = true
        };

        var queryableCompanies = allCompanies.AsQueryable();
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
        _mapperMock.Setup(x => x.ToDto(targetCompany)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDefault, Is.True);
        Assert.That(targetCompany.IsDefault, Is.True);
        Assert.That(nonDefaultCompanies.All(c => !c.IsDefault), Is.True);
        
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}