using ExpenseVista.API.DTOs.Pagination;

namespace ExpenseVista.API.Utilities
{
    public static class IQueryableExtentions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, PaginationDTO pagination)
        {
            return queryable.Skip((pagination.Page - 1) * pagination.RecordsPerPage).Take(pagination.RecordsPerPage);
        }
    }
}
