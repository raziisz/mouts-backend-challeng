using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Queries;
using Ambev.DeveloperEvaluation.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class ProductRepository(DefaultContext context) : IProductRepository
{
    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await context.Products.AddAsync(product, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return product;
    }
    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<PagedResult<Product>> ListAsync(ProductListQuery request, CancellationToken cancellationToken = default)
    {
        var query = context.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Title)) query = ApplyTextFilter(query, request.Title, useTitle: true);
        if (!string.IsNullOrWhiteSpace(request.Category)) query = ApplyTextFilter(query, request.Category, useTitle: false);
        if (request.Price.HasValue) query = query.Where(x => x.Price == request.Price.Value);
        if (request.MinPrice.HasValue) query = query.Where(x => x.Price >= request.MinPrice.Value);
        if (request.MaxPrice.HasValue) query = query.Where(x => x.Price <= request.MaxPrice.Value);

        query = ApplyOrder(query, request.Page.Order);
        return query.ToPagedResultAsync(request.Page, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> ListCategoriesAsync(CancellationToken cancellationToken = default) =>
        await context.Products.AsNoTracking().Select(x => x.Category).Distinct().OrderBy(x => x).ToListAsync(cancellationToken);

    private static IQueryable<Product> ApplyTextFilter(IQueryable<Product> query, string filter, bool useTitle)
    {
        var normalized = filter.Trim('*').ToLower();
        var startsWithWildcard = filter.StartsWith('*');
        var endsWithWildcard = filter.EndsWith('*');

        if (startsWithWildcard && endsWithWildcard) return useTitle ? query.Where(x => x.Title.ToLower().Contains(normalized)) : query.Where(x => x.Category.ToLower().Contains(normalized));
        if (startsWithWildcard) return useTitle ? query.Where(x => x.Title.ToLower().EndsWith(normalized)) : query.Where(x => x.Category.ToLower().EndsWith(normalized));
        if (endsWithWildcard) return useTitle ? query.Where(x => x.Title.ToLower().StartsWith(normalized)) : query.Where(x => x.Category.ToLower().StartsWith(normalized));
        return useTitle ? query.Where(x => x.Title.ToLower() == normalized) : query.Where(x => x.Category.ToLower() == normalized);
    }

    private static IQueryable<Product> ApplyOrder(IQueryable<Product> query, string? order)
    {
        if (string.IsNullOrWhiteSpace(order)) return query.OrderBy(x => x.Id);
        IOrderedQueryable<Product>? result = null;
        foreach (var part in order.Replace("\"", string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var desc = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            var field = tokens[0].ToLowerInvariant();
            result = result is null ? Order(query, field, desc) : ThenOrder(result, field, desc);
        }
        return result ?? query.OrderBy(x => x.Id);
    }

    private static IOrderedQueryable<Product> Order(IQueryable<Product> query, string field, bool desc) => field switch
    {
        "price" => desc ? query.OrderByDescending(x => x.Price) : query.OrderBy(x => x.Price),
        "title" => desc ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title),
        "category" => desc ? query.OrderByDescending(x => x.Category) : query.OrderBy(x => x.Category),
        _ => desc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
    };

    private static IOrderedQueryable<Product> ThenOrder(IOrderedQueryable<Product> query, string field, bool desc) => field switch
    {
        "price" => desc ? query.ThenByDescending(x => x.Price) : query.ThenBy(x => x.Price),
        "title" => desc ? query.ThenByDescending(x => x.Title) : query.ThenBy(x => x.Title),
        "category" => desc ? query.ThenByDescending(x => x.Category) : query.ThenBy(x => x.Category),
        _ => desc ? query.ThenByDescending(x => x.Id) : query.ThenBy(x => x.Id)
    };

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default) { context.Products.Update(product); await context.SaveChangesAsync(cancellationToken); }
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await context.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return false;
        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
