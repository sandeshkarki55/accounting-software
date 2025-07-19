using AccountingApi.DTOs;

using System.Linq.Expressions;

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

            // This is a basic implementation and can be extended for more complex sorting
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, sorting.OrderBy);
            var lambda = Expression.Lambda(property, parameter);

            var methodName = sorting.Descending ? "OrderByDescending" : "OrderBy";

            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), property.Type },
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
