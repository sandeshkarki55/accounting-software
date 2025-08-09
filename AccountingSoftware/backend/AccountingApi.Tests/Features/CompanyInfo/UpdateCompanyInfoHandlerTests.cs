using AccountingApi.DTOs;
using AccountingApi.Features.CompanyInfo;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.CompanyInfo;

public class UpdateCompanyInfoHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<CompanyInfoMapper> _mapperMock = null!;
    private UpdateCompanyInfoHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _mapperMock = new Mock<CompanyInfoMapper>();
        
        _handler = new UpdateCompanyInfoHandler(
            _contextMock.Object,
            _mapperMock.Object);
    }

    [Test]
    public async Task Handle_UpdatesCompanyInfo_WhenValidRequest()
    {
        // Arrange
        var companyId = 1;
        var updateCompanyInfoDto = new CreateCompanyInfoDto
        {
            CompanyName = "Updated Company Name",
            LegalName = "Updated Legal Name",
            Email = "updated@company.com",
            Phone = "987-654-3210",
            Address = "456 Updated St",
            City = "Updated City",
            State = "Updated State",
            PostalCode = "54321",
            Country = "Updated Country",
            TaxNumber = "987654321",
            IsDefault = false
        };

        var command = new UpdateCompanyInfoCommand(companyId, updateCompanyInfoDto);

        var existingCompany = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "Original Company Name",
            LegalName = "Original Legal Name",
            Email = "original@company.com",
            IsDefault = false
        };

        var expectedDto = new CompanyInfoDto
        {
            Id = companyId,
            CompanyName = "Updated Company Name",
            LegalName = "Updated Legal Name",
            Email = "updated@company.com",
            Phone = "987-654-3210",
            IsDefault = false
        };

        var companies = new List<AccountingApi.Models.CompanyInfo> { existingCompany };
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
        _mapperMock.Setup(x => x.UpdateEntity(existingCompany, updateCompanyInfoDto));
        _mapperMock.Setup(x => x.ToDto(existingCompany)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CompanyName, Is.EqualTo("Updated Company Name"));
        Assert.That(result.Email, Is.EqualTo("updated@company.com"));
        
        _mapperMock.Verify(x => x.UpdateEntity(existingCompany, updateCompanyInfoDto), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(x => x.ToDto(existingCompany), Times.Once);
    }

    [Test]
    public async Task Handle_ThrowsKeyNotFoundException_WhenCompanyNotFound()
    {
        // Arrange
        var companyId = 999;
        var updateCompanyInfoDto = new CreateCompanyInfoDto
        {
            CompanyName = "Updated Company Name",
            IsDefault = false
        };

        var command = new UpdateCompanyInfoCommand(companyId, updateCompanyInfoDto);

        var companies = new List<AccountingApi.Models.CompanyInfo>
        {
            new() { Id = 1, CompanyName = "Other Company", IsDefault = false }
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
    public async Task Handle_UnsetsOtherDefaults_WhenSettingCompanyAsDefault()
    {
        // Arrange
        var companyId = 1;
        var updateCompanyInfoDto = new CreateCompanyInfoDto
        {
            CompanyName = "Company to be Default",
            IsDefault = true
        };

        var command = new UpdateCompanyInfoCommand(companyId, updateCompanyInfoDto);

        var targetCompany = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "Company to be Default",
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
            CompanyName = "Company to be Default",
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
        _mapperMock.Setup(x => x.UpdateEntity(targetCompany, updateCompanyInfoDto));
        _mapperMock.Setup(x => x.ToDto(targetCompany)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDefault, Is.True);
        Assert.That(otherDefaultCompanies.All(c => !c.IsDefault), Is.True);
        
        _mapperMock.Verify(x => x.UpdateEntity(targetCompany, updateCompanyInfoDto), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_DoesNotUnsetOtherDefaults_WhenCompanyAlreadyDefault()
    {
        // Arrange
        var companyId = 1;
        var updateCompanyInfoDto = new CreateCompanyInfoDto
        {
            CompanyName = "Already Default Company",
            IsDefault = true
        };

        var command = new UpdateCompanyInfoCommand(companyId, updateCompanyInfoDto);

        var targetCompany = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "Already Default Company",
            IsDefault = true // Already default
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
        _mapperMock.Setup(x => x.UpdateEntity(targetCompany, updateCompanyInfoDto));
        _mapperMock.Setup(x => x.ToDto(targetCompany)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDefault, Is.True);
        Assert.That(otherCompanies.All(c => !c.IsDefault), Is.True);
        
        _mapperMock.Verify(x => x.UpdateEntity(targetCompany, updateCompanyInfoDto), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_DoesNotUnsetOtherDefaults_WhenNotSettingAsDefault()
    {
        // Arrange
        var companyId = 1;
        var updateCompanyInfoDto = new CreateCompanyInfoDto
        {
            CompanyName = "Non-Default Company",
            IsDefault = false
        };

        var command = new UpdateCompanyInfoCommand(companyId, updateCompanyInfoDto);

        var targetCompany = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "Non-Default Company",
            IsDefault = false
        };

        var otherDefaultCompany = new AccountingApi.Models.CompanyInfo
        {
            Id = 2,
            CompanyName = "Default Company",
            IsDefault = true
        };

        var allCompanies = new List<AccountingApi.Models.CompanyInfo> { targetCompany, otherDefaultCompany };

        var expectedDto = new CompanyInfoDto
        {
            Id = companyId,
            CompanyName = "Non-Default Company",
            IsDefault = false
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
        _mapperMock.Setup(x => x.UpdateEntity(targetCompany, updateCompanyInfoDto));
        _mapperMock.Setup(x => x.ToDto(targetCompany)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDefault, Is.False);
        Assert.That(otherDefaultCompany.IsDefault, Is.True); // Should remain unchanged
        
        _mapperMock.Verify(x => x.UpdateEntity(targetCompany, updateCompanyInfoDto), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_UpdatesNonDefaultCompanyToDefault_UnsettingOthers()
    {
        // Arrange
        var companyId = 3;
        var updateCompanyInfoDto = new CreateCompanyInfoDto
        {
            CompanyName = "New Default Company",
            Email = "newdefault@company.com",
            IsDefault = true
        };

        var command = new UpdateCompanyInfoCommand(companyId, updateCompanyInfoDto);

        var targetCompany = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "New Default Company",
            Email = "old@company.com",
            IsDefault = false
        };

        var currentDefaultCompanies = new List<AccountingApi.Models.CompanyInfo>
        {
            new() { Id = 1, CompanyName = "Old Default 1", IsDefault = true },
            new() { Id = 2, CompanyName = "Old Default 2", IsDefault = true }
        };

        var allCompanies = new List<AccountingApi.Models.CompanyInfo> { targetCompany }
            .Concat(currentDefaultCompanies).ToList();

        var expectedDto = new CompanyInfoDto
        {
            Id = companyId,
            CompanyName = "New Default Company",
            Email = "newdefault@company.com",
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
        _mapperMock.Setup(x => x.UpdateEntity(targetCompany, updateCompanyInfoDto));
        _mapperMock.Setup(x => x.ToDto(targetCompany)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDefault, Is.True);
        Assert.That(currentDefaultCompanies.All(c => !c.IsDefault), Is.True);
        
        _mapperMock.Verify(x => x.UpdateEntity(targetCompany, updateCompanyInfoDto), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_UpdatesCompanyFields_WithoutChangingDefaultStatus()
    {
        // Arrange
        var companyId = 1;
        var updateCompanyInfoDto = new CreateCompanyInfoDto
        {
            CompanyName = "Updated Company Name",
            LegalName = "Updated Legal Name",
            Email = "updated@test.com",
            Phone = "555-1234",
            IsDefault = false
        };

        var command = new UpdateCompanyInfoCommand(companyId, updateCompanyInfoDto);

        var existingCompany = new AccountingApi.Models.CompanyInfo
        {
            Id = companyId,
            CompanyName = "Original Name",
            LegalName = "Original Legal Name",
            Email = "original@test.com",
            IsDefault = false
        };

        var companies = new List<AccountingApi.Models.CompanyInfo> { existingCompany };

        var expectedDto = new CompanyInfoDto
        {
            Id = companyId,
            CompanyName = "Updated Company Name",
            LegalName = "Updated Legal Name",
            Email = "updated@test.com",
            Phone = "555-1234",
            IsDefault = false
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
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapperMock.Setup(x => x.UpdateEntity(existingCompany, updateCompanyInfoDto));
        _mapperMock.Setup(x => x.ToDto(existingCompany)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CompanyName, Is.EqualTo("Updated Company Name"));
        Assert.That(result.Email, Is.EqualTo("updated@test.com"));
        Assert.That(result.Phone, Is.EqualTo("555-1234"));
        Assert.That(result.IsDefault, Is.False);
        
        _mapperMock.Verify(x => x.UpdateEntity(existingCompany, updateCompanyInfoDto), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}