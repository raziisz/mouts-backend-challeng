using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class CartRepository(DefaultContext context) : ICartRepository
{
    public async Task<Cart> CreateAsync(Cart cart, CancellationToken cancellationToken = default) { await context.Carts.AddAsync(cart, cancellationToken); await context.SaveChangesAsync(cancellationToken); return cart; }
    public Task<Cart?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => context.Carts.Include(x => x.Items).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task<IReadOnlyList<Cart>> ListAsync(CancellationToken cancellationToken = default) => await context.Carts.Include(x => x.Items).AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken);
    public async Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default) { context.Carts.Update(cart); await context.SaveChangesAsync(cancellationToken); }
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cart = await context.Carts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cart is null) return false;
        context.Carts.Remove(cart);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
