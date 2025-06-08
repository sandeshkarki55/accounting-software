# Mapping Infrastructure

This folder contains the mapping infrastructure that handles conversions between domain models and DTOs throughout the application. This design removes mapping logic from handlers and controllers, making the code more maintainable and testable.

## Architecture

### Base Interfaces

- **`IMapper<TEntity, TDto>`**: Base interface for simple entity-to-DTO mapping
- **`IEntityMapper<TEntity, TDto, TCreateDto, TUpdateDto>`**: Extended interface that includes creation and update operations

### Available Mappers

- **`AccountMapper`**: Maps Account entities and related DTOs
- **`CustomerMapper`**: Maps Customer entities and related DTOs  
- **`CompanyInfoMapper`**: Maps CompanyInfo entities and related DTOs
- **`InvoiceMapper`**: Maps Invoice entities and related DTOs
- **`InvoiceItemMapper`**: Maps InvoiceItem entities and related DTOs
- **`JournalEntryMapper`**: Maps JournalEntry entities and related DTOs
- **`JournalEntryLineMapper`**: Maps JournalEntryLine entities and related DTOs

## Usage Examples

### Basic Entity-to-DTO Mapping

```csharp
public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto?>
{
    private readonly AccountingDbContext _context;
    private readonly AccountMapper _accountMapper;

    public GetAccountByIdQueryHandler(AccountingDbContext context, AccountMapper accountMapper)
    {
        _context = context;
        _accountMapper = accountMapper;
    }

    public async Task<AccountDto?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .Include(a => a.ParentAccount)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (account == null)
            return null;

        return _accountMapper.ToDto(account);
    }
}
```

### Collection Mapping

```csharp
public async Task<List<AccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
{
    var accounts = await _context.Accounts
        .Include(a => a.ParentAccount)
        .OrderBy(a => a.AccountCode)
        .ToListAsync(cancellationToken);

    return _accountMapper.ToDto(accounts).ToList();
}
```

### Create Entity from DTO

```csharp
public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
{
    // Business validation...
    
    // Create new account entity using mapper
    var account = _accountMapper.ToEntity(request.Account);

    _context.Accounts.Add(account);
    await _context.SaveChangesAsync(cancellationToken);

    // Return the created account as DTO using mapper
    return _accountMapper.ToDto(account);
}
```

### Update Entity from DTO

```csharp
public async Task<bool> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
{
    var account = await _context.Accounts
        .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

    if (account == null)
        return false;

    // Update account using mapper
    _accountMapper.UpdateEntity(account, request.Account);

    await _context.SaveChangesAsync(cancellationToken);
    return true;
}
```

## Benefits

### 1. **Separation of Concerns**
- Mapping logic is separated from business logic
- Handlers focus on business rules and data access
- Mappers focus solely on data transformation

### 2. **Code Reusability**
- Mapping logic can be reused across multiple handlers
- Consistent mapping behavior throughout the application
- Reduces code duplication

### 3. **Maintainability**
- Changes to mapping logic are centralized
- Easy to test mapping logic in isolation
- Clear responsibility boundaries

### 4. **Type Safety**
- Compile-time checking of mapping operations
- IntelliSense support for mapping methods
- Reduces runtime mapping errors

### 5. **Testability**
- Mappers can be unit tested independently
- Mock mappers can be used in handler tests
- Clear interfaces for dependency injection

## Dependency Injection Setup

The mapping services are registered in `Program.cs`:

```csharp
// Add Mapping Services
builder.Services.AddMappingServices();
```

This extension method registers all mappers as scoped services, making them available for dependency injection throughout the application.

## Best Practices

### 1. **Include Navigation Properties**
When fetching entities for mapping, include necessary navigation properties:

```csharp
var account = await _context.Accounts
    .Include(a => a.ParentAccount)
    .Include(a => a.SubAccounts)
    .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
```

### 2. **Handle Null Values**
Mappers handle null navigation properties gracefully:

```csharp
ParentAccountName = entity.ParentAccount?.AccountName,
```

### 3. **Business Logic in Handlers**
Keep business logic in handlers, not in mappers:

```csharp
// Good: Business validation in handler
if (existingAccount != null)
{
    throw new InvalidOperationException($"Account with code '{request.Account.AccountCode}' already exists.");
}

var account = _accountMapper.ToEntity(request.Account);
```

### 4. **Calculated Properties**
Mappers can include calculated properties:

```csharp
Level = CalculateLevel(entity),
SubTotal = entity.Items?.Sum(i => i.Amount) ?? 0,
```

## Error Handling

Some mappers include specific error handling for business rules:

```csharp
public void UpdateEntity(JournalEntry entity, object updateDto)
{
    // Journal entries are typically immutable once created
    throw new NotSupportedException("Journal entries cannot be updated once created.");
}
```

## Extension Points

To add a new mapper:

1. Create a new mapper class implementing the appropriate interface
2. Register it in `MappingServiceExtensions.cs`
3. Inject it into handlers that need mapping functionality

This architecture provides a clean, maintainable foundation for handling all entity-DTO conversions in the application.