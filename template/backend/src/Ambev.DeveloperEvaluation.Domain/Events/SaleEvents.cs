using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public interface ISaleEvent { }
public interface IEventPublisher
{
    Task PublishAsync(ISaleEvent saleEvent, CancellationToken cancellationToken = default);
}
public record SaleCreated(Sale Sale) : ISaleEvent;
public record SaleModified(Sale Sale) : ISaleEvent;
public record SaleCancelled(int SaleId) : ISaleEvent;
public record ItemCancelled(int SaleId, int ItemId) : ISaleEvent;
