using Ambev.DeveloperEvaluation.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Queries;

public static class PaginationExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, PageQuery page, CancellationToken cancellationToken = default)
    {
        var totalItems = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip(page.Offset)
            .Take(page.NormalizedSize)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Create(data, totalItems, page);
    }
}
