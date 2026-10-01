using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository(DefaultContext context) : ISaleRepository
{
    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default) { await context.Sales.AddAsync(sale, cancellationToken); await context.SaveChangesAsync(cancellationToken); return sale; }
    public Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => context.Sales.Include(x => x.Items).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task<IReadOnlyList<Sale>> ListAsync(CancellationToken cancellationToken = default) => await context.Sales.Include(x => x.Items).AsNoTracking().OrderByDescending(x => x.Date).ToListAsync(cancellationToken);
    public async Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default) { context.Sales.Update(sale); await context.SaveChangesAsync(cancellationToken); }
}
