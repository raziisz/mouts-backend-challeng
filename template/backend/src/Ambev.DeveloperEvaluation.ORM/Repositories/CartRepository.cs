using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Common.Persistence;
using Ambev.DeveloperEvaluation.ORM.Queries;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class CartRepository(DefaultContext context) : ICartRepository
{
    public async Task<Cart> CreateAsync(Cart cart, CancellationToken cancellationToken = default) { await context.Carts.AddAsync(cart, cancellationToken); await context.SaveChangesAsync(cancellationToken); return cart; }
    public Task<Cart?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => context.Carts.Include(x => x.Items).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<PagedResult<Cart>> ListAsync(CartListQuery request, CancellationToken cancellationToken = default)
    {
        var query = context.Carts.Include(x => x.Items).AsNoTracking().AsQueryable();
        if (request.UserId.HasValue) query = query.Where(x => x.UserId == request.UserId.Value);
        if (request.Date.HasValue) query = query.Where(x => x.Date.Date == request.Date.Value.Date);
        if (request.MinDate.HasValue) query = query.Where(x => x.Date >= request.MinDate.Value);
        if (request.MaxDate.HasValue) query = query.Where(x => x.Date <= request.MaxDate.Value);

        query = ApplyOrder(query, request.Page.Order);
        return query.ToPagedResultAsync(request.Page, cancellationToken);
    }

    private static IQueryable<Cart> ApplyOrder(IQueryable<Cart> query, string? order)
    {
        if (string.IsNullOrWhiteSpace(order)) return query.OrderBy(x => x.Id);
        IOrderedQueryable<Cart>? result = null;
        foreach (var part in order.Replace("\"", string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var desc = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            var field = tokens[0].ToLowerInvariant();
            result = result is null ? Order(query, field, desc) : ThenOrder(result, field, desc);
        }
        return result ?? query.OrderBy(x => x.Id);
    }

    private static IOrderedQueryable<Cart> Order(IQueryable<Cart> query, string field, bool desc) => field switch
    {
        "userid" => desc ? query.OrderByDescending(x => x.UserId) : query.OrderBy(x => x.UserId),
        "date" => desc ? query.OrderByDescending(x => x.Date) : query.OrderBy(x => x.Date),
        _ => desc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
    };

    private static IOrderedQueryable<Cart> ThenOrder(IOrderedQueryable<Cart> query, string field, bool desc) => field switch
    {
        "userid" => desc ? query.ThenByDescending(x => x.UserId) : query.ThenBy(x => x.UserId),
        "date" => desc ? query.ThenByDescending(x => x.Date) : query.ThenBy(x => x.Date),
        _ => desc ? query.ThenByDescending(x => x.Id) : query.ThenBy(x => x.Id)
    };
    public async Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        var current = await context.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == cart.Id, cancellationToken);

        if (current is null)
            throw new KeyNotFoundException($"Cart {cart.Id} not found.");

        current.UserId = cart.UserId;
        current.Date = cart.Date;
        context.CartItems.RemoveRange(current.Items);
        current.Items.Clear();

        foreach (var item in cart.Items)
        {
            current.AddItem(item.ProductId, item.Quantity);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cart = await context.Carts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cart is null) return false;
        context.Carts.Remove(cart);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
