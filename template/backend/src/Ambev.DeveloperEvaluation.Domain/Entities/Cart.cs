using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Cart : BaseEntity
{
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CartItem> Items { get; set; } = [];

    public void AddItem(int productId, int quantity)
    {
        if (productId <= 0) throw new DomainException("Product ID must be greater than zero.");
        if (quantity <= 0) throw new DomainException("Cart quantity must be greater than zero.");

        var item = Items.FirstOrDefault(x => x.ProductId == productId);
        if (item is null) Items.Add(new CartItem { ProductId = productId, Quantity = quantity });
        else item.Quantity += quantity;
    }

    public void ReplaceItems(IEnumerable<CartItem> items)
    {
        Items.Clear();
        foreach (var item in items) AddItem(item.ProductId, item.Quantity);
    }
}

public class CartItem : BaseEntity
{
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
