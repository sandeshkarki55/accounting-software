using AccountingApi.Controllers;
using AccountingApi.DTOs;
using AccountingApi.Features.Accounts;
using AccountingApi.Models;

using Microsoft.AspNetCore.Mvc;

using Moq;

using MyMediator;

namespace AccountingApi.Tests.Controllers;

public class AccountsControllerTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private AccountsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _mediatorMock = new Mock<IMediator>(MockBehavior.Strict);
        _controller = new AccountsController(_mediatorMock.Object);
    }

    [Test]
    public async Task GetAccounts_ReturnsOk_WithPagedResults()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var sorting = new SortingParams { SortField = "AccountName", SortOrder = "asc" };
        var filtering = new AccountFilteringParams { AccountType = AccountType.Asset };
        
        var expectedResult = new PagedResult<AccountDto>
        {
            Items = new List<AccountDto>
            {
                new() { Id = 1, AccountName = "Cash", AccountType = AccountType.Asset },
                new() { Id = 2, AccountName = "Accounts Receivable", AccountType = AccountType.Asset }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllAccountsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetAccounts(pagination, sorting, filtering);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedResult));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetAllAccountsQuery>(q => 
                q.Pagination == pagination && 
                q.Sorting == sorting && 
                q.Filtering == filtering), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetAccountsHierarchy_ReturnsOk_WithAccountList()
    {
        // Arrange
        var expectedAccounts = new List<AccountDto>
        {
            new() { Id = 1, AccountName = "Assets", AccountType = AccountType.Asset, Level = 0 },
            new() { Id = 2, AccountName = "Cash", AccountType = AccountType.Asset, Level = 1, ParentAccountId = 1 }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAccountsHierarchyQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAccounts);

        // Act
        var result = await _controller.GetAccountsHierarchy();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedAccounts));
        
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetAccountsHierarchyQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetAccount_ReturnsOk_WhenAccountExists()
    {
        // Arrange
        const int accountId = 1;
        var expectedAccount = new AccountDto { Id = accountId, AccountName = "Cash", AccountType = AccountType.Asset };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAccountByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAccount);

        // Act
        var result = await _controller.GetAccount(accountId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedAccount));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetAccountByIdQuery>(q => q.Id == accountId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetAccount_ReturnsNotFound_WhenAccountDoesNotExist()
    {
        // Arrange
        const int accountId = 99;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAccountByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountDto?)null);

        // Act
        var result = await _controller.GetAccount(accountId);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        
        var response = notFoundResult.Value;
        Assert.That(response, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetAccountByIdQuery>(q => q.Id == accountId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateAccount_ReturnsCreatedAtAction_WhenSuccessful()
    {
        // Arrange
        var createAccountDto = new CreateAccountDto
        {
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = AccountType.Asset,
            Description = "Cash account"
        };

        var createdAccount = new AccountDto
        {
            Id = 1,
            AccountCode = "1000",
            AccountName = "Cash",
            AccountType = AccountType.Asset,
            Description = "Cash account"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdAccount);

        // Act
        var result = await _controller.CreateAccount(createAccountDto);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult.ActionName, Is.EqualTo(nameof(AccountsController.GetAccount)));
        Assert.That(createdResult.RouteValues!["id"], Is.EqualTo(createdAccount.Id));
        Assert.That(createdResult.Value, Is.EqualTo(createdAccount));
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateAccountCommand>(c => c.Account == createAccountDto), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateAccount_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        const int accountId = 1;
        var updateAccountDto = new UpdateAccountDto
        {
            AccountName = "Updated Cash",
            Description = "Updated description",
            IsActive = true
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateAccount(accountId, updateAccountDto);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateAccountCommand>(c => c.Id == accountId && c.UpdateAccount == updateAccountDto), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateAccount_ReturnsNotFound_WhenAccountDoesNotExist()
    {
        // Arrange
        const int accountId = 99;
        var updateAccountDto = new UpdateAccountDto
        {
            AccountName = "Updated Account",
            Description = "Updated description",
            IsActive = true
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateAccount(accountId, updateAccountDto);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        
        var response = notFoundResult.Value;
        Assert.That(response, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateAccountCommand>(c => c.Id == accountId && c.UpdateAccount == updateAccountDto), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteAccount_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        const int accountId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAccount(accountId);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.That(noContentResult, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteAccountCommand>(c => c.Id == accountId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteAccount_ReturnsNotFound_WhenAccountDoesNotExist()
    {
        // Arrange
        const int accountId = 99;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteAccount(accountId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        
        var response = notFoundResult.Value;
        Assert.That(response, Is.Not.Null);
        
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteAccountCommand>(c => c.Id == accountId), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}