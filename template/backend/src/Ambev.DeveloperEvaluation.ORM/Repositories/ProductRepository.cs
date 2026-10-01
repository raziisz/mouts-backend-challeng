using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
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
    public async Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken = default) => await context.Products.AsNoTracking().OrderBy(x => x.Id).ToListAsync(cancellationToken);
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
