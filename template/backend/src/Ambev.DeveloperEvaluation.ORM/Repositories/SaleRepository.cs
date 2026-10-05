using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Common.Persistence;
using Ambev.DeveloperEvaluation.ORM.Queries;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository(DefaultContext context) : ISaleRepository
{
    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default) { await context.Sales.AddAsync(sale, cancellationToken); await context.SaveChangesAsync(cancellationToken); return sale; }
    public Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => context.Sales.Include(x => x.Items).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<PagedResult<Sale>> ListAsync(SaleListQuery request, CancellationToken cancellationToken = default)
    {
        var query = context.Sales.Include(x => x.Items).AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.SaleNumber)) query = ApplyTextFilter(query, request.SaleNumber);
        if (request.CustomerId.HasValue) query = query.Where(x => x.Customer.Id == request.CustomerId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<SaleStatus>(request.Status, ignoreCase: true, out var saleStatus)) return Task.FromResult(PagedResult<Sale>.Create([], 0, request.Page));
            query = query.Where(x => x.Status == saleStatus);
        }
        if (request.Date.HasValue) query = query.Where(x => x.Date.Date == request.Date.Value.Date);
        if (request.MinDate.HasValue) query = query.Where(x => x.Date >= request.MinDate.Value);
        if (request.MaxDate.HasValue) query = query.Where(x => x.Date <= request.MaxDate.Value);

        query = ApplyOrder(query, request.Page.Order);
        return query.ToPagedResultAsync(request.Page, cancellationToken);
    }

    private static IQueryable<Sale> ApplyTextFilter(IQueryable<Sale> query, string filter)
    {
        var normalized = filter.Trim('*').ToLower();
        var startsWithWildcard = filter.StartsWith('*');
        var endsWithWildcard = filter.EndsWith('*');
        if (startsWithWildcard && endsWithWildcard) return query.Where(x => x.SaleNumber.ToLower().Contains(normalized));
        if (startsWithWildcard) return query.Where(x => x.SaleNumber.ToLower().EndsWith(normalized));
        if (endsWithWildcard) return query.Where(x => x.SaleNumber.ToLower().StartsWith(normalized));
        return query.Where(x => x.SaleNumber.ToLower() == normalized);
    }

    private static IQueryable<Sale> ApplyOrder(IQueryable<Sale> query, string? order)
    {
        if (string.IsNullOrWhiteSpace(order)) return query.OrderByDescending(x => x.Date);
        IOrderedQueryable<Sale>? result = null;
        foreach (var part in order.Replace("\"", string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var desc = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            var field = tokens[0].ToLowerInvariant();
            result = result is null ? Order(query, field, desc) : ThenOrder(result, field, desc);
        }
        return result ?? query.OrderByDescending(x => x.Date);
    }

    private static IOrderedQueryable<Sale> Order(IQueryable<Sale> query, string field, bool desc) => field switch
    {
        "salenumber" => desc ? query.OrderByDescending(x => x.SaleNumber) : query.OrderBy(x => x.SaleNumber),
        "date" => desc ? query.OrderByDescending(x => x.Date) : query.OrderBy(x => x.Date),
        "status" => desc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
        _ => desc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
    };

    private static IOrderedQueryable<Sale> ThenOrder(IOrderedQueryable<Sale> query, string field, bool desc) => field switch
    {
        "salenumber" => desc ? query.ThenByDescending(x => x.SaleNumber) : query.ThenBy(x => x.SaleNumber),
        "date" => desc ? query.ThenByDescending(x => x.Date) : query.ThenBy(x => x.Date),
        "status" => desc ? query.ThenByDescending(x => x.Status) : query.ThenBy(x => x.Status),
        _ => desc ? query.ThenByDescending(x => x.Id) : query.ThenBy(x => x.Id)
    };
    public async Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        var current = await context.Sales
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == sale.Id, cancellationToken);

        if (current is null)
            throw new KeyNotFoundException($"Sale {sale.Id} not found.");

        current.SaleNumber = sale.SaleNumber;
        current.Date = sale.Date;
        current.Customer.Id = sale.Customer.Id;
        current.Customer.Description = sale.Customer.Description;
        current.Branch.Id = sale.Branch.Id;
        current.Branch.Description = sale.Branch.Description;

        if (sale.Status == SaleStatus.Cancelled)
            current.Cancel();

        context.SaleItems.RemoveRange(current.Items);
        current.Items.Clear();

        foreach (var item in sale.Items)
        {
            current.AddItem(item.ProductId, item.ProductDescription, item.UnitPrice, item.Quantity);
            current.Items[^1].IsCancelled = item.IsCancelled;
        }

        await context.SaveChangesAsync(cancellationToken);

        for (var index = 0; index < sale.Items.Count; index++)
        {
            sale.Items[index].Id = current.Items[index].Id;
            sale.Items[index].SaleId = current.Id;
        }
    }
}
