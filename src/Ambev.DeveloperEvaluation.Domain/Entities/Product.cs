using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Product : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public decimal RatingRate { get; set; }
    public int RatingCount { get; set; }

    public void ValidateBusinessRules()
    {
        if (string.IsNullOrWhiteSpace(Title)) throw new DomainException("Product title is required.");
        if (Price <= 0) throw new DomainException("Product price must be greater than zero.");
        if (string.IsNullOrWhiteSpace(Category)) throw new DomainException("Product category is required.");
        if (RatingRate is < 0 or > 5) throw new DomainException("Product rating must be between 0 and 5.");
        if (RatingCount < 0) throw new DomainException("Product rating count cannot be negative.");
    }
}
