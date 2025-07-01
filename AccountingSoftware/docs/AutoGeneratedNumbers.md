# Auto-Generated Invoice Numbers and Customer Codes

## Overview

This feature implements automatic generation of invoice numbers and customer codes using SQL Server sequences. Numbers are generated server-side and follow a consistent format.

## Implementation Details

### Backend Changes

#### 1. Database Sequences
- `InvoiceSequence`: Generates sequential numbers for invoices (starts at 1)
- `CustomerSequence`: Generates sequential numbers for customers (starts at 1)

#### 2. Number Generation Service
- `INumberGenerationService`: Interface for dependency injection
- `NumberGenerationService`: Implementation using SQL Server sequences
- Registered in DI container in `Program.cs`

#### 3. Updated Models and DTOs
- `CreateInvoiceDto`: Removed `InvoiceNumber` property
- `CreateCustomerDto`: Removed `CustomerCode` property
- Numbers are now auto-generated and cannot be manually specified

#### 4. Command Handlers
- `CreateInvoiceCommandHandler`: Uses `INumberGenerationService` to generate invoice numbers
- `CreateCustomerCommandHandler`: Uses `INumberGenerationService` to generate customer codes

### Frontend Changes

#### 1. Updated TypeScript Types
- Removed number fields from create DTOs
- API calls send data without number fields

#### 2. UI Components
- **InvoiceModal**: Removed invoice number input, shows generated number in view mode
- **CustomerModal**: Removed customer code input, shows generated code in edit mode

## Number Formats

- **Invoice Numbers**: `INV-{sequence}` (e.g., INV-1, INV-2, INV-3, ...)
- **Customer Codes**: `CUST-{sequence}` (e.g., CUST-1, CUST-2, CUST-3, ...)

## Migration

### Database Migration: `AddNumberSequences`
```sql
-- Create sequence for invoice numbers
CREATE SEQUENCE InvoiceSequence START WITH 1 INCREMENT BY 1;

-- Create sequence for customer codes  
CREATE SEQUENCE CustomerSequence START WITH 1 INCREMENT BY 1;
```

### Running the Migration
```bash
dotnet ef database update
```

## Usage

### Creating a New Customer
1. Frontend sends `CreateCustomerDto` without `customerCode`
2. Backend generates code using `CustomerSequence`
3. Returns customer with auto-generated code like "CUST-1"

### Creating a New Invoice
1. Frontend sends `CreateInvoiceDto` without `invoiceNumber`
2. Backend generates number using `InvoiceSequence`
3. Returns invoice with auto-generated number like "INV-1"

## Benefits

✅ **Consistency**: All numbers follow the same format  
✅ **Uniqueness**: SQL Server sequences guarantee unique numbers  
✅ **Concurrency Safe**: Multiple users can create records simultaneously  
✅ **User Friendly**: No need for users to think of numbers  
✅ **Scalable**: Sequences can handle high-volume operations  

## Error Handling

- If sequence generation fails, the operation will rollback
- Connection management ensures proper cleanup
- Service layer handles all sequence-related errors

## Future Enhancements

- Configurable number formats
- Support for company-specific prefixes
- Reset sequences by year/month
- Custom starting numbers for existing data migration
