using AccountingApi.Controllers;
using AccountingApi.DTOs;
using AccountingApi.Features.CompanyInfo;

using Microsoft.AspNetCore.Mvc;

using Moq;

using MyMediator;

namespace AccountingApi.Tests.Controllers;

public class CompanyInfoControllerTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private CompanyInfoController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
        _controller = new CompanyInfoController(_mediatorMock.Object);
    }

    [Test]
    public async Task GetCompanyInfos_ReturnsOk_WithPagedResults()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sorting = new SortingParams { SortField = "CompanyName", SortOrder = "asc" };
        var filtering = new CompanyInfoFilteringParams { SearchTerm = "test" };
        
        var expectedResult = new PagedResult<CompanyInfoDto>
        {
            Items = new List<CompanyInfoDto>
            {
                new() { Id = 1, CompanyName = "Test Company", Email = "test@company.com" },
                new() { Id = 2, CompanyName = "Another Company", Email = "info@another.com" }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllCompanyInfosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetCompanyInfos(pagination, sorting, filtering);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedResult));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetAllCompanyInfosQuery>(q => 
                q.Pagination == pagination && 
                q.Sorting == sorting && 
                q.Filtering == filtering), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateCompanyInfo_ReturnsCreatedAtAction_WhenSuccessful()
    {
        // Arrange
        var createCompanyInfoDto = new CreateCompanyInfoDto
        {
            CompanyName = "New Company",
            Email = "new@company.com",
            Phone = "123-456-7890",
            Address = "123 Business St",
            City = "Business City",
            State = "Business State",
            PostalCode = "12345",
            Country = "Country"
        };

        var createdCompanyInfo = new CompanyInfoDto
        {
            Id = 1,
            CompanyName = "New Company",
            Email = "new@company.com",
            Phone = "123-456-7890",
            Address = "123 Business St",
            City = "Business City",
            State = "Business State",
            PostalCode = "12345",
            Country = "Country",
            IsDefault = false
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateCompanyInfoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCompanyInfo);

        // Act
        var result = await _controller.CreateCompanyInfo(createCompanyInfoDto);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult.ActionName, Is.EqualTo(nameof(CompanyInfoController.GetCompanyInfos)));
        Assert.That(createdResult.RouteValues!["id"], Is.EqualTo(createdCompanyInfo.Id));
        Assert.That(createdResult.Value, Is.EqualTo(createdCompanyInfo));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateCompanyInfoCommand>(c => c.CompanyInfo == createCompanyInfoDto), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateCompanyInfo_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        const int companyInfoId = 1;
        var updateCompanyInfoDto = new CreateCompanyInfoDto
        {
            CompanyName = "Updated Company",
            Email = "updated@company.com",
            Phone = "987-654-3210"
        };

        var updatedCompanyInfo = new CompanyInfoDto
        {
            Id = companyInfoId,
            CompanyName = "Updated Company",
            Email = "updated@company.com",
            Phone = "987-654-3210"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCompanyInfoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedCompanyInfo);

        // Act
        var result = await _controller.UpdateCompanyInfo(companyInfoId, updateCompanyInfoDto);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(updatedCompanyInfo));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateCompanyInfoCommand>(c => c.Id == companyInfoId && c.CompanyInfo == updateCompanyInfoDto), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteCompanyInfo_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        const int companyInfoId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteCompanyInfoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCompanyInfo(companyInfoId);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteCompanyInfoCommand>(c => c.Id == companyInfoId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteCompanyInfo_ReturnsNotFound_WhenCompanyInfoDoesNotExist()
    {
        // Arrange
        const int companyInfoId = 99;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteCompanyInfoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteCompanyInfo(companyInfoId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        
        var response = notFoundResult.Value;
        Assert.That(response, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteCompanyInfoCommand>(c => c.Id == companyInfoId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SetDefaultCompany_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        const int companyInfoId = 1;

        var defaultCompanyInfo = new CompanyInfoDto
        {
            Id = companyInfoId,
            CompanyName = "Default Company",
            Email = "default@company.com",
            IsDefault = true
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<SetDefaultCompanyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultCompanyInfo);

        // Act
        var result = await _controller.SetDefaultCompany(companyInfoId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(defaultCompanyInfo));
        
        var companyInfo = okResult.Value as CompanyInfoDto;
        Assert.That(companyInfo!.IsDefault, Is.True);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<SetDefaultCompanyCommand>(c => c.Id == companyInfoId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}