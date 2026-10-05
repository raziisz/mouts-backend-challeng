using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Common.Persistence;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ICartRepository
{
    Task<Cart> CreateAsync(Cart cart, CancellationToken cancellationToken = default);
    Task<Cart?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<Cart>> ListAsync(CartListQuery query, CancellationToken cancellationToken = default);
    Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
