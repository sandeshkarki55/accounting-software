using System.Linq.Expressions;

using AccountingApi.DTOs;

namespace AccountingApi.Infrastructure.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, PaginationParams pagination)
        {
            return query.Skip((pagination.PageNumber - 1) * pagination.PageSize).Take(pagination.PageSize);
        }

        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, SortingParams sorting)
        {
            if (string.IsNullOrEmpty(sorting.OrderBy))
            {
                // Apply a default sort if none is specified
                // This needs to be handled in the specific query handler
                return query;
            }

            // Validate the property exists on the type
            var propertyInfo = typeof(T).GetProperty(sorting.OrderBy,
                System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (propertyInfo == null)
            {
                throw new ArgumentException(
                    $"Sort property '{sorting.OrderBy}' does not exist on type '{typeof(T).Name}'. " +
                    $"Allowed properties: {string.Join(", ", typeof(T).GetProperties().Select(p => p.Name))}");
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyInfo);
            var lambda = Expression.Lambda(property, parameter);

            var methodName = sorting.Descending ? "OrderByDescending" : "OrderBy";

            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), propertyInfo.PropertyType },
                query.Expression,
                Expression.Quote(lambda));

            return query.Provider.CreateQuery<T>(resultExpression);
        }

        // Note: Filtering logic is often very specific to the entity and filter parameters,
        // so a generic extension might not be suitable. It's often better to apply filtering
        // directly in the query handler or use a specification pattern.
        // However, for simple cases like a general search term, a basic extension could be created.
    }
}