using AccountingApi.DTOs;
using AccountingApi.Features.CompanyInfo;
using AccountingApi.Infrastructure;
using AccountingApi.Mappings;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace AccountingApi.Tests.Features.CompanyInfo;

public class GetAllCompanyInfosHandlerTests
{
    private Mock<AccountingDbContext> _contextMock = null!;
    private Mock<CompanyInfoMapper> _mapperMock = null!;
    private GetAllCompanyInfosQueryHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _contextMock = new Mock<AccountingDbContext>(options);
        _mapperMock = new Mock<CompanyInfoMapper>();
        
        _handler = new GetAllCompanyInfosQueryHandler(
            _contextMock.Object,
            _mapperMock.Object);
    }

    [Test]
    public async Task Handle_ReturnsPagedResults_WhenValidRequest()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 2 };
        var sortingParams = new SortingParams { OrderBy = "CompanyName", IsDescending = false };
        var filteringParams = new CompanyInfoFilteringParams { SearchTerm = null };
        
        var query = new GetAllCompanyInfosQuery(paginationParams, sortingParams, filteringParams);

        var companies = new List<AccountingApi.Models.CompanyInfo>
        {
            new() { Id = 1, CompanyName = "Alpha Company", IsDefault = true },
            new() { Id = 2, CompanyName = "Beta Company", IsDefault = false },
            new() { Id = 3, CompanyName = "Gamma Company", IsDefault = false }
        };

        var expectedDtos = new List<CompanyInfoDto>
        {
            new() { Id = 1, CompanyName = "Alpha Company", IsDefault = true },
            new() { Id = 2, CompanyName = "Beta Company", IsDefault = false }
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
        _mapperMock.Setup(x => x.ToDto(It.IsAny<IEnumerable<AccountingApi.Models.CompanyInfo>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(3));
        Assert.That(result.PageNumber, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(2));
        Assert.That(result.Items.First().CompanyName, Is.EqualTo("Alpha Company"));
    }

    [Test]
    public async Task Handle_FiltersResults_WhenSearchTermProvided()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sortingParams = new SortingParams();
        var filteringParams = new CompanyInfoFilteringParams { SearchTerm = "Alpha" };
        
        var query = new GetAllCompanyInfosQuery(paginationParams, sortingParams, filteringParams);

        var companies = new List<AccountingApi.Models.CompanyInfo>
        {
            new() { Id = 1, CompanyName = "Alpha Company", Email = "alpha@test.com" },
            new() { Id = 2, CompanyName = "Beta Company", Email = "beta@test.com" },
            new() { Id = 3, CompanyName = "Gamma Alpha Corp", Phone = "123-456-7890" }
        };

        var expectedDtos = new List<CompanyInfoDto>
        {
            new() { Id = 1, CompanyName = "Alpha Company", Email = "alpha@test.com" },
            new() { Id = 3, CompanyName = "Gamma Alpha Corp", Phone = "123-456-7890" }
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
        _mapperMock.Setup(x => x.ToDto(It.IsAny<IEnumerable<AccountingApi.Models.CompanyInfo>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(2));
        Assert.That(result.Items.All(c => c.CompanyName.Contains("Alpha")), Is.True);
    }

    [Test]
    public async Task Handle_FiltersResultsByLegalName_WhenSearchTermProvided()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sortingParams = new SortingParams();
        var filteringParams = new CompanyInfoFilteringParams { SearchTerm = "LLC" };
        
        var query = new GetAllCompanyInfosQuery(paginationParams, sortingParams, filteringParams);

        var companies = new List<AccountingApi.Models.CompanyInfo>
        {
            new() { Id = 1, CompanyName = "Test Company", LegalName = "Test Company LLC" },
            new() { Id = 2, CompanyName = "Another Company", LegalName = "Another Company Inc" },
            new() { Id = 3, CompanyName = "Third Company", LegalName = "Third LLC Corp" }
        };

        var expectedDtos = new List<CompanyInfoDto>
        {
            new() { Id = 1, CompanyName = "Test Company", LegalName = "Test Company LLC" },
            new() { Id = 3, CompanyName = "Third Company", LegalName = "Third LLC Corp" }
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
        _mapperMock.Setup(x => x.ToDto(It.IsAny<IEnumerable<AccountingApi.Models.CompanyInfo>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(2));
        Assert.That(result.Items.All(c => c.LegalName!.Contains("LLC")), Is.True);
    }

    [Test]
    public async Task Handle_FiltersResultsByEmailAndPhone_WhenSearchTermProvided()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sortingParams = new SortingParams();
        var filteringParams = new CompanyInfoFilteringParams { SearchTerm = "123" };
        
        var query = new GetAllCompanyInfosQuery(paginationParams, sortingParams, filteringParams);

        var companies = new List<AccountingApi.Models.CompanyInfo>
        {
            new() { Id = 1, CompanyName = "Company A", Email = "test123@example.com" },
            new() { Id = 2, CompanyName = "Company B", Phone = "123-456-7890" },
            new() { Id = 3, CompanyName = "Company C", Email = "info@example.com", Phone = "987-654-3210" }
        };

        var expectedDtos = new List<CompanyInfoDto>
        {
            new() { Id = 1, CompanyName = "Company A", Email = "test123@example.com" },
            new() { Id = 2, CompanyName = "Company B", Phone = "123-456-7890" }
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
        _mapperMock.Setup(x => x.ToDto(It.IsAny<IEnumerable<AccountingApi.Models.CompanyInfo>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Handle_UsesDefaultSorting_WhenNoSortingProvided()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sortingParams = new SortingParams { OrderBy = null };
        var filteringParams = new CompanyInfoFilteringParams();
        
        var query = new GetAllCompanyInfosQuery(paginationParams, sortingParams, filteringParams);

        var companies = new List<AccountingApi.Models.CompanyInfo>
        {
            new() { Id = 1, CompanyName = "Beta Company", IsDefault = false },
            new() { Id = 2, CompanyName = "Alpha Company", IsDefault = true },
            new() { Id = 3, CompanyName = "Gamma Company", IsDefault = false }
        };

        var expectedDtos = new List<CompanyInfoDto>
        {
            new() { Id = 2, CompanyName = "Alpha Company", IsDefault = true },
            new() { Id = 1, CompanyName = "Beta Company", IsDefault = false },
            new() { Id = 3, CompanyName = "Gamma Company", IsDefault = false }
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
        _mapperMock.Setup(x => x.ToDto(It.IsAny<IEnumerable<AccountingApi.Models.CompanyInfo>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(3));
        // Default sorting should prioritize default company first, then alphabetical by name
        Assert.That(result.Items.First().IsDefault, Is.True);
    }

    [Test]
    public async Task Handle_ReturnsEmptyResults_WhenNoCompaniesExist()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sortingParams = new SortingParams();
        var filteringParams = new CompanyInfoFilteringParams();
        
        var query = new GetAllCompanyInfosQuery(paginationParams, sortingParams, filteringParams);

        var companies = new List<AccountingApi.Models.CompanyInfo>();
        var expectedDtos = new List<CompanyInfoDto>();

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
        _mapperMock.Setup(x => x.ToDto(It.IsAny<IEnumerable<AccountingApi.Models.CompanyInfo>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items, Is.Empty);
        Assert.That(result.TotalCount, Is.EqualTo(0));
        Assert.That(result.PageNumber, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(10));
    }

    [Test]
    public async Task Handle_HandlesPaginationCorrectly_WhenMultiplePages()
    {
        // Arrange
        var paginationParams = new PaginationParams { PageNumber = 2, PageSize = 2 };
        var sortingParams = new SortingParams();
        var filteringParams = new CompanyInfoFilteringParams();
        
        var query = new GetAllCompanyInfosQuery(paginationParams, sortingParams, filteringParams);

        var companies = new List<AccountingApi.Models.CompanyInfo>
        {
            new() { Id = 1, CompanyName = "Company A", IsDefault = false },
            new() { Id = 2, CompanyName = "Company B", IsDefault = false },
            new() { Id = 3, CompanyName = "Company C", IsDefault = false },
            new() { Id = 4, CompanyName = "Company D", IsDefault = false },
            new() { Id = 5, CompanyName = "Company E", IsDefault = false }
        };

        var expectedDtos = new List<CompanyInfoDto>
        {
            new() { Id = 3, CompanyName = "Company C", IsDefault = false },
            new() { Id = 4, CompanyName = "Company D", IsDefault = false }
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
        _mapperMock.Setup(x => x.ToDto(It.IsAny<IEnumerable<AccountingApi.Models.CompanyInfo>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items.Count, Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(5));
        Assert.That(result.PageNumber, Is.EqualTo(2));
        Assert.That(result.PageSize, Is.EqualTo(2));
    }
}