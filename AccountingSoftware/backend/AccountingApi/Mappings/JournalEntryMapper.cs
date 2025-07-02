using AccountingApi.Models;
using AccountingApi.DTOs;

namespace AccountingApi.Mappings;

/// <summary>
/// Mapper for JournalEntry entity and related DTOs
/// </summary>
public class JournalEntryMapper(JournalEntryLineMapper lineMapper) : IEntityMapper<JournalEntry, JournalEntryDto, CreateJournalEntryDto, object>
{

    /// <summary>
    /// Maps a JournalEntry entity to JournalEntryDto
    /// </summary>
    /// <param name="entity">The JournalEntry entity to map</param>
    /// <returns>The mapped JournalEntryDto</returns>
    public JournalEntryDto ToDto(JournalEntry entity)
    {
        return new JournalEntryDto
        {
            Id = entity.Id,
            EntryNumber = entity.EntryNumber,
            TransactionDate = entity.TransactionDate,
            Description = entity.Description,
            Reference = entity.Reference,
            TotalAmount = entity.TotalAmount,
            IsPosted = entity.IsPosted,
            Lines = entity.Lines?.Select(lineMapper.ToDto).ToList() ?? []
        };
    }

    /// <summary>
    /// Maps a collection of JournalEntry entities to JournalEntryDto collection
    /// </summary>
    /// <param name="entities">The JournalEntry entities to map</param>
    /// <returns>The mapped JournalEntryDto collection</returns>
    public IEnumerable<JournalEntryDto> ToDto(IEnumerable<JournalEntry> entities)
    {
        return entities.Select(ToDto);
    }

    /// <summary>
    /// Maps a CreateJournalEntryDto to JournalEntry entity
    /// </summary>
    /// <param name="createDto">The CreateJournalEntryDto to map</param>
    /// <returns>The mapped JournalEntry entity</returns>
    public JournalEntry ToEntity(CreateJournalEntryDto createDto)
    {
        var entry = new JournalEntry
        {
            TransactionDate = createDto.TransactionDate,
            Description = createDto.Description,
            Reference = createDto.Reference,
            IsPosted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Map the lines
        entry.Lines = createDto.Lines?.Select(lineDto => lineMapper.ToEntity(lineDto, entry.Id)).ToList() ?? [];
        
        // Calculate total amount
        entry.TotalAmount = entry.Lines.Sum(l => Math.Max(l.DebitAmount, l.CreditAmount));

        return entry;
    }

    /// <summary>
    /// Journal entries typically don't support updates, but this is required by the interface
    /// </summary>
    public void UpdateEntity(JournalEntry entity, object updateDto)
    {
        // Journal entries are typically immutable once created
        throw new NotSupportedException("Journal entries cannot be updated once created.");
    }
}

/// <summary>
/// Mapper for JournalEntryLine entity and related DTOs
/// </summary>
public class JournalEntryLineMapper
{
    /// <summary>
    /// Maps a JournalEntryLine entity to JournalEntryLineDto
    /// </summary>
    /// <param name="entity">The JournalEntryLine entity to map</param>
    /// <returns>The mapped JournalEntryLineDto</returns>
    public JournalEntryLineDto ToDto(JournalEntryLine entity)
    {
        return new JournalEntryLineDto
        {
            Id = entity.Id,
            AccountId = entity.AccountId,
            AccountCode = entity.Account?.AccountCode ?? string.Empty,
            AccountName = entity.Account?.AccountName ?? string.Empty,
            DebitAmount = entity.DebitAmount,
            CreditAmount = entity.CreditAmount,
            Description = entity.Description
        };
    }

    /// <summary>
    /// Maps a CreateJournalEntryLineDto to JournalEntryLine entity
    /// </summary>
    /// <param name="createDto">The CreateJournalEntryLineDto to map</param>
    /// <param name="journalEntryId">The ID of the parent journal entry</param>
    /// <returns>The mapped JournalEntryLine entity</returns>
    public JournalEntryLine ToEntity(CreateJournalEntryLineDto createDto, int journalEntryId)
    {
        return new JournalEntryLine
        {
            JournalEntryId = journalEntryId,
            AccountId = createDto.AccountId,
            DebitAmount = createDto.DebitAmount,
            CreditAmount = createDto.CreditAmount,
            Description = createDto.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}