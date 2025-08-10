using AccountingApi.DTOs;
using AccountingApi.Features.CompanyInfo;
using AccountingApi.Models;
using AccountingApi.Tests.TestHelpers;

namespace AccountingApi.Tests.Features.CompanyInfo;

public class CreateCompanyInfoHandlerTests : BaseTestWithInMemoryDb
{
    private CreateCompanyInfoCommandHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        
        _handler = new CreateCompanyInfoCommandHandler(
            Context,
            CompanyInfoMapper,
            CurrentUserServiceMock.Object);
    }

    protected override void SeedTestData()
    {
        AddTestCompanies();
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

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CompanyName, Is.EqualTo("Test Company"));
        Assert.That(result.Email, Is.EqualTo("info@testcompany.com"));
        Assert.That(result.LegalName, Is.EqualTo("Test Company LLC"));
        Assert.That(result.IsDefault, Is.False);
        
        // Verify the company was created in the database
        var createdCompany = Context.CompanyInfos.FirstOrDefault(c => c.CompanyName == "Test Company");
        Assert.That(createdCompany, Is.Not.Null);
        Assert.That(createdCompany.CreatedBy, Is.EqualTo("test-user-id"));
        Assert.That(createdCompany.UpdatedBy, Is.EqualTo("test-user-id"));
    }

    [Test]
    public async Task Handle_UnsetsOtherDefaults_WhenSettingAsDefault()
    {
        // Arrange
        // Modify existing test companies to have some as default
        var existingCompanies = Context.CompanyInfos.ToList();
        foreach (var company in existingCompanies)
        {
            company.IsDefault = true;
            company.UpdatedBy = "old-user";
        }
        Context.SaveChanges();

        var createCompanyInfoDto = new CreateCompanyInfoDto
        {
            CompanyName = "New Default Company",
            IsDefault = true
        };

        var command = new CreateCompanyInfoCommand(createCompanyInfoDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDefault, Is.True);
        
        // Verify the new company was created and set as default
        var newCompany = Context.CompanyInfos.FirstOrDefault(c => c.CompanyName == "New Default Company");
        Assert.That(newCompany, Is.Not.Null);
        Assert.That(newCompany.IsDefault, Is.True);
        
        // Verify existing companies are no longer default
        var otherCompanies = Context.CompanyInfos.Where(c => c.CompanyName != "New Default Company").ToList();
        Assert.That(otherCompanies.All(c => !c.IsDefault), Is.True);
        Assert.That(otherCompanies.All(c => c.UpdatedBy == "test-user-id"), Is.True);
    }

    [Test]
    public async Task Handle_DoesNotUnsetDefaults_WhenNotSettingAsDefault()
    {
        // Arrange
        // Set existing companies as default
        var existingCompanies = Context.CompanyInfos.ToList();
        foreach (var company in existingCompanies)
        {
            company.IsDefault = true;
        }
        Context.SaveChanges();

        var createCompanyInfoDto = new CreateCompanyInfoDto
        {
            CompanyName = "Non-Default Company",
            IsDefault = false
        };

        var command = new CreateCompanyInfoCommand(createCompanyInfoDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsDefault, Is.False);
        
        // Verify the new company was created as non-default
        var newCompany = Context.CompanyInfos.FirstOrDefault(c => c.CompanyName == "Non-Default Company");
        Assert.That(newCompany, Is.Not.Null);
        Assert.That(newCompany.IsDefault, Is.False);
        
        // Verify existing companies remain default (should not be affected)
        var existingCompaniesAfter = Context.CompanyInfos.Where(c => c.CompanyName != "Non-Default Company").ToList();
        Assert.That(existingCompaniesAfter.All(c => c.IsDefault), Is.True);
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

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var createdCompany = Context.CompanyInfos.FirstOrDefault(c => c.CompanyName == "Audit Test Company");
        Assert.That(createdCompany, Is.Not.Null);
        Assert.That(createdCompany.CreatedBy, Is.EqualTo("test-user-id"));
        Assert.That(createdCompany.UpdatedBy, Is.EqualTo("test-user-id"));
        Assert.That(createdCompany.CreatedAt, Is.Not.Null);
        Assert.That(createdCompany.UpdatedAt, Is.Not.Null);
    }
}