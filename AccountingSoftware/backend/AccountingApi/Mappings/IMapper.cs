namespace AccountingApi.Mappings;

/// <summary>
/// Base interface for mapping between entities and DTOs
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TDto">The DTO type</typeparam>
public interface IMapper<TEntity, TDto>
{
    /// <summary>
    /// Maps an entity to a DTO
    /// </summary>
    /// <param name="entity">The entity to map</param>
    /// <returns>The mapped DTO</returns>
    TDto ToDto(TEntity entity);

    /// <summary>
    /// Maps a collection of entities to DTOs
    /// </summary>
    /// <param name="entities">The entities to map</param>
    /// <returns>The mapped DTOs</returns>
    IEnumerable<TDto> ToDto(IEnumerable<TEntity> entities);
}

/// <summary>
/// Extended mapper interface for entities that support creation from DTOs
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TDto">The DTO type</typeparam>
/// <typeparam name="TCreateDto">The create DTO type</typeparam>
/// <typeparam name="TUpdateDto">The update DTO type</typeparam>
public interface IEntityMapper<TEntity, TDto, TCreateDto, TUpdateDto> : IMapper<TEntity, TDto>
{
    /// <summary>
    /// Maps a create DTO to an entity
    /// </summary>
    /// <param name="createDto">The create DTO to map</param>
    /// <returns>The mapped entity</returns>
    TEntity ToEntity(TCreateDto createDto);

    /// <summary>
    /// Updates an existing entity with data from an update DTO
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <param name="updateDto">The update DTO containing new data</param>
    void UpdateEntity(TEntity entity, TUpdateDto updateDto);
}