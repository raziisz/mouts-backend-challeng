using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class ExternalIdentity
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public ExternalIdentity Customer { get; set; } = new();
    public ExternalIdentity Branch { get; set; } = new();
    public SaleStatus Status { get; private set; } = SaleStatus.Active;
    public List<SaleItem> Items { get; set; } = [];
    public decimal TotalAmount => Items.Where(x => !x.IsCancelled).Sum(x => x.TotalAmount);

    public void AddItem(int productId, string productDescription, decimal unitPrice, int quantity)
    {
        if (productId <= 0) throw new DomainException("Product ID must be greater than zero.");
        if (unitPrice <= 0) throw new DomainException("Unit price must be greater than zero.");
        if (quantity <= 0) throw new DomainException("Quantity must be greater than zero.");
        if (quantity > 20) throw new DomainException("It is not possible to sell more than 20 identical items.");

        var item = new SaleItem
        {
            ProductId = productId,
            ProductDescription = productDescription,
            UnitPrice = unitPrice,
            Quantity = quantity
        };
        item.Recalculate();
        Items.Add(item);
    }

    public void ReplaceItems(IEnumerable<SaleItemInput> items)
    {
        var wasCancelled = Status == SaleStatus.Cancelled;
        Items.Clear();
        foreach (var item in items)
            AddItem(item.ProductId, item.ProductDescription, item.UnitPrice, item.Quantity);

        if (wasCancelled)
        {
            foreach (var item in Items)
                item.IsCancelled = true;
        }
    }

    public void Cancel()
    {
        Status = SaleStatus.Cancelled;
        foreach (var item in Items) item.IsCancelled = true;
    }

    public void CancelItem(int itemId)
    {
        var item = Items.FirstOrDefault(x => x.Id == itemId)
            ?? throw new DomainException("Sale item was not found.");
        item.IsCancelled = true;
    }
}

public sealed record SaleItemInput(int ProductId, string ProductDescription, decimal UnitPrice, int Quantity);

public class SaleItem : BaseEntity
{
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public string ProductDescription { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal DiscountRate { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; set; }

    public void Recalculate()
    {
        DiscountRate = DiscountPolicy.GetRate(Quantity);
        var gross = UnitPrice * Quantity;
        DiscountAmount = Math.Round(gross * DiscountRate, 2, MidpointRounding.AwayFromZero);
        TotalAmount = gross - DiscountAmount;
    }
}

public static class DiscountPolicy
{
    public static decimal GetRate(int quantity)
    {
        if (quantity < 0) throw new DomainException("Quantity cannot be negative.");
        if (quantity > 20) throw new DomainException("It is not possible to sell more than 20 identical items.");
        return quantity >= 10 ? 0.20m : quantity >= 4 ? 0.10m : 0m;
    }
}
