using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Common.Persistence;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products
{
    public class ProductResult
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public RatingResult Rating { get; set; } = new();
    }

    public class RatingResult { public decimal Rate { get; set; } public int Count { get; set; } }

    public record CreateProductCommand(string Title, decimal Price, string Description, string Category, string Image, decimal RatingRate, int RatingCount) : IRequest<ProductResult>;
    public record GetProductsQuery(int Page = 1, int Size = 10, string? Order = null, string? Title = null, string? Category = null, decimal? Price = null, decimal? MinPrice = null, decimal? MaxPrice = null) : IRequest<PagedResult<ProductResult>>;
    public record GetProductCategoriesQuery : IRequest<IReadOnlyList<string>>;
    public record GetProductQuery(int Id) : IRequest<ProductResult>;
    public record UpdateProductCommand(int Id, string Title, decimal Price, string Description, string Category, string Image, decimal RatingRate, int RatingCount) : IRequest<ProductResult>;
    public record DeleteProductCommand(int Id) : IRequest<bool>;

    public static class ProductMappings
    {
        public static ProductResult ToResult(Product x) => new() { Id = x.Id, Title = x.Title, Price = x.Price, Description = x.Description, Category = x.Category, Image = x.Image, Rating = new RatingResult { Rate = x.RatingRate, Count = x.RatingCount } };
        public static PagedResult<ProductResult> ToPage(PagedResult<Product> page) => new() { Data = page.Data.Select(ToResult).ToList(), TotalItems = page.TotalItems, CurrentPage = page.CurrentPage, TotalPages = page.TotalPages };
    }

    public class CreateProductHandler(IProductRepository repository) : IRequestHandler<CreateProductCommand, ProductResult>
    {
        public async Task<ProductResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product { Title = request.Title, Price = request.Price, Description = request.Description, Category = request.Category, Image = request.Image, RatingRate = request.RatingRate, RatingCount = request.RatingCount };
            product.ValidateBusinessRules();
            return ProductMappings.ToResult(await repository.CreateAsync(product, cancellationToken));
        }
    }
    public class GetProductsHandler(IProductRepository repository) : IRequestHandler<GetProductsQuery, PagedResult<ProductResult>>
    {
        public async Task<PagedResult<ProductResult>> Handle(GetProductsQuery request, CancellationToken cancellationToken) => ProductMappings.ToPage(await repository.ListAsync(new ProductListQuery(new PageQuery(request.Page, request.Size, request.Order), request.Title, request.Category, request.Price, request.MinPrice, request.MaxPrice), cancellationToken));
    }
    public class GetProductCategoriesHandler(IProductRepository repository) : IRequestHandler<GetProductCategoriesQuery, IReadOnlyList<string>>
    {
        public Task<IReadOnlyList<string>> Handle(GetProductCategoriesQuery request, CancellationToken cancellationToken) => repository.ListCategoriesAsync(cancellationToken);
    }
    public class GetProductHandler(IProductRepository repository) : IRequestHandler<GetProductQuery, ProductResult>
    {
        public async Task<ProductResult> Handle(GetProductQuery request, CancellationToken cancellationToken) => ProductMappings.ToResult(await repository.GetByIdAsync(request.Id, cancellationToken) ?? throw new KeyNotFoundException($"Product {request.Id} not found."));
    }
    public class UpdateProductHandler(IProductRepository repository) : IRequestHandler<UpdateProductCommand, ProductResult>
    {
        public async Task<ProductResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await repository.GetByIdAsync(request.Id, cancellationToken) ?? throw new KeyNotFoundException($"Product {request.Id} not found.");
            product.Title = request.Title; product.Price = request.Price; product.Description = request.Description; product.Category = request.Category; product.Image = request.Image; product.RatingRate = request.RatingRate; product.RatingCount = request.RatingCount;
            product.ValidateBusinessRules();
            await repository.UpdateAsync(product, cancellationToken);
            return ProductMappings.ToResult(product);
        }
    }
    public class DeleteProductHandler(IProductRepository repository) : IRequestHandler<DeleteProductCommand, bool>
    {
        public Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken) => repository.DeleteAsync(request.Id, cancellationToken);
    }
}

namespace Ambev.DeveloperEvaluation.Application.Carts
{
    public class CartItemResult { public int ProductId { get; set; } public int Quantity { get; set; } }
    public class CartResult { public int Id { get; set; } public int UserId { get; set; } public DateTime Date { get; set; } public List<CartItemResult> Products { get; set; } = []; }
    public record CreateCartCommand(int UserId, DateTime Date, List<CartItemResult> Products) : IRequest<CartResult>;
    public record GetCartsQuery(int Page = 1, int Size = 10, string? Order = null, int? UserId = null, DateTime? Date = null, DateTime? MinDate = null, DateTime? MaxDate = null) : IRequest<PagedResult<CartResult>>;
    public record GetCartQuery(int Id) : IRequest<CartResult>;
    public record UpdateCartCommand(int Id, int UserId, DateTime Date, List<CartItemResult> Products) : IRequest<CartResult>;
    public record DeleteCartCommand(int Id) : IRequest<bool>;

    public static class CartMappings
    {
        public static CartResult ToResult(Cart x) => new() { Id = x.Id, UserId = x.UserId, Date = x.Date, Products = x.Items.Select(i => new CartItemResult { ProductId = i.ProductId, Quantity = i.Quantity }).ToList() };
        public static PagedResult<CartResult> ToPage(PagedResult<Cart> page) => new() { Data = page.Data.Select(ToResult).ToList(), TotalItems = page.TotalItems, CurrentPage = page.CurrentPage, TotalPages = page.TotalPages };
        public static Cart ToEntity(int userId, DateTime date, IEnumerable<CartItemResult> products) { var cart = new Cart { UserId = userId, Date = date }; cart.ReplaceItems(products.Select(x => new CartItem { ProductId = x.ProductId, Quantity = x.Quantity })); return cart; }
    }
    public class CreateCartHandler(ICartRepository repository) : IRequestHandler<CreateCartCommand, CartResult>
    {
        public async Task<CartResult> Handle(CreateCartCommand request, CancellationToken cancellationToken) => CartMappings.ToResult(await repository.CreateAsync(CartMappings.ToEntity(request.UserId, request.Date, request.Products), cancellationToken));
    }
    public class GetCartsHandler(ICartRepository repository) : IRequestHandler<GetCartsQuery, PagedResult<CartResult>>
    {
        public async Task<PagedResult<CartResult>> Handle(GetCartsQuery request, CancellationToken cancellationToken) => CartMappings.ToPage(await repository.ListAsync(new CartListQuery(new PageQuery(request.Page, request.Size, request.Order), request.UserId, request.Date, request.MinDate, request.MaxDate), cancellationToken));
    }
    public class GetCartHandler(ICartRepository repository) : IRequestHandler<GetCartQuery, CartResult>
    {
        public async Task<CartResult> Handle(GetCartQuery request, CancellationToken cancellationToken) => CartMappings.ToResult(await repository.GetByIdAsync(request.Id, cancellationToken) ?? throw new KeyNotFoundException($"Cart {request.Id} not found."));
    }
    public class UpdateCartHandler(ICartRepository repository) : IRequestHandler<UpdateCartCommand, CartResult>
    {
        public async Task<CartResult> Handle(UpdateCartCommand request, CancellationToken cancellationToken)
        {
            var cart = await repository.GetByIdAsync(request.Id, cancellationToken) ?? throw new KeyNotFoundException($"Cart {request.Id} not found.");
            cart.UserId = request.UserId; cart.Date = request.Date; cart.ReplaceItems(request.Products.Select(x => new CartItem { ProductId = x.ProductId, Quantity = x.Quantity }));
            await repository.UpdateAsync(cart, cancellationToken);
            return CartMappings.ToResult(cart);
        }
    }
    public class DeleteCartHandler(ICartRepository repository) : IRequestHandler<DeleteCartCommand, bool>
    {
        public Task<bool> Handle(DeleteCartCommand request, CancellationToken cancellationToken) => repository.DeleteAsync(request.Id, cancellationToken);
    }
}

namespace Ambev.DeveloperEvaluation.Application.Sales
{
    public class SaleItemResult { public int Id { get; set; } public int ProductId { get; set; } public string ProductDescription { get; set; } = string.Empty; public int Quantity { get; set; } public decimal UnitPrice { get; set; } public decimal DiscountRate { get; set; } public decimal DiscountAmount { get; set; } public decimal TotalAmount { get; set; } public bool IsCancelled { get; set; } }
    public class SaleResult { public int Id { get; set; } public string SaleNumber { get; set; } = string.Empty; public DateTime Date { get; set; } public ExternalIdentity Customer { get; set; } = new(); public ExternalIdentity Branch { get; set; } = new(); public string Status { get; set; } = string.Empty; public decimal TotalAmount { get; set; } public List<SaleItemResult> Products { get; set; } = []; }
    public class SaleItemInputModel { public int ProductId { get; set; } public string ProductDescription { get; set; } = string.Empty; public decimal UnitPrice { get; set; } public int Quantity { get; set; } }
    public record CreateSaleCommand(string SaleNumber, DateTime Date, int CustomerId, string CustomerDescription, int BranchId, string BranchDescription, List<SaleItemInputModel> Products) : IRequest<SaleResult>;
    public record GetSalesQuery(int Page = 1, int Size = 10, string? Order = null, string? SaleNumber = null, string? Status = null, DateTime? Date = null, DateTime? MinDate = null, DateTime? MaxDate = null) : IRequest<PagedResult<SaleResult>>;
    public record GetSaleQuery(int Id) : IRequest<SaleResult>;
    public record UpdateSaleCommand(int Id, string SaleNumber, DateTime Date, int CustomerId, string CustomerDescription, int BranchId, string BranchDescription, List<SaleItemInputModel> Products) : IRequest<SaleResult>;
    public record CancelSaleCommand(int Id) : IRequest<SaleResult>;
    public record CancelSaleItemCommand(int SaleId, int ItemId) : IRequest<SaleResult>;

    public static class SaleMappings
    {
        public static SaleResult ToResult(Sale x) => new() { Id = x.Id, SaleNumber = x.SaleNumber, Date = x.Date, Customer = x.Customer, Branch = x.Branch, Status = x.Status.ToString(), TotalAmount = x.TotalAmount, Products = x.Items.Select(i => new SaleItemResult { Id = i.Id, ProductId = i.ProductId, ProductDescription = i.ProductDescription, Quantity = i.Quantity, UnitPrice = i.UnitPrice, DiscountRate = i.DiscountRate, DiscountAmount = i.DiscountAmount, TotalAmount = i.TotalAmount, IsCancelled = i.IsCancelled }).ToList() };
        public static PagedResult<SaleResult> ToPage(PagedResult<Sale> page) => new() { Data = page.Data.Select(ToResult).ToList(), TotalItems = page.TotalItems, CurrentPage = page.CurrentPage, TotalPages = page.TotalPages };
        public static Sale ToEntity(string number, DateTime date, int customerId, string customerDescription, int branchId, string branchDescription, IEnumerable<SaleItemInputModel> products)
        {
            var sale = new Sale { SaleNumber = number, Date = date, Customer = new ExternalIdentity { Id = customerId, Description = customerDescription }, Branch = new ExternalIdentity { Id = branchId, Description = branchDescription } };
            sale.ReplaceItems(products.Select(x => new SaleItemInput(x.ProductId, x.ProductDescription, x.UnitPrice, x.Quantity)));
            return sale;
        }
    }
    public class CreateSaleHandler(ISaleRepository repository, IEventPublisher eventPublisher) : IRequestHandler<CreateSaleCommand, SaleResult>
    {
        public async Task<SaleResult> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            var sale = await repository.CreateAsync(SaleMappings.ToEntity(request.SaleNumber, request.Date, request.CustomerId, request.CustomerDescription, request.BranchId, request.BranchDescription, request.Products), cancellationToken);
            await eventPublisher.PublishAsync(new SaleCreated(sale), cancellationToken);
            return SaleMappings.ToResult(sale);
        }
    }
    public class GetSalesHandler(ISaleRepository repository) : IRequestHandler<GetSalesQuery, PagedResult<SaleResult>>
    {
        public async Task<PagedResult<SaleResult>> Handle(GetSalesQuery request, CancellationToken cancellationToken) => SaleMappings.ToPage(await repository.ListAsync(new SaleListQuery(new PageQuery(request.Page, request.Size, request.Order), request.SaleNumber, request.Status, request.Date, request.MinDate, request.MaxDate), cancellationToken));
    }
    public class GetSaleHandler(ISaleRepository repository) : IRequestHandler<GetSaleQuery, SaleResult>
    {
        public async Task<SaleResult> Handle(GetSaleQuery request, CancellationToken cancellationToken) => SaleMappings.ToResult(await repository.GetByIdAsync(request.Id, cancellationToken) ?? throw new KeyNotFoundException($"Sale {request.Id} not found."));
    }
    public class UpdateSaleHandler(ISaleRepository repository, IEventPublisher eventPublisher) : IRequestHandler<UpdateSaleCommand, SaleResult>
    {
        public async Task<SaleResult> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
        {
            var sale = await repository.GetByIdAsync(request.Id, cancellationToken) ?? throw new KeyNotFoundException($"Sale {request.Id} not found.");
            sale.SaleNumber = request.SaleNumber; sale.Date = request.Date; sale.Customer = new ExternalIdentity { Id = request.CustomerId, Description = request.CustomerDescription }; sale.Branch = new ExternalIdentity { Id = request.BranchId, Description = request.BranchDescription }; sale.ReplaceItems(request.Products.Select(x => new SaleItemInput(x.ProductId, x.ProductDescription, x.UnitPrice, x.Quantity)));
            await repository.UpdateAsync(sale, cancellationToken);
            await eventPublisher.PublishAsync(new SaleModified(sale), cancellationToken);
            return SaleMappings.ToResult(sale);
        }
    }
    public class CancelSaleHandler(ISaleRepository repository, IEventPublisher eventPublisher) : IRequestHandler<CancelSaleCommand, SaleResult>
    {
        public async Task<SaleResult> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
        {
            var sale = await repository.GetByIdAsync(request.Id, cancellationToken) ?? throw new KeyNotFoundException($"Sale {request.Id} not found.");
            sale.Cancel(); await repository.UpdateAsync(sale, cancellationToken); await eventPublisher.PublishAsync(new SaleCancelled(sale.Id), cancellationToken); return SaleMappings.ToResult(sale);
        }
    }
    public class CancelSaleItemHandler(ISaleRepository repository, IEventPublisher eventPublisher) : IRequestHandler<CancelSaleItemCommand, SaleResult>
    {
        public async Task<SaleResult> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
        {
            var sale = await repository.GetByIdAsync(request.SaleId, cancellationToken) ?? throw new KeyNotFoundException($"Sale {request.SaleId} not found.");
            sale.CancelItem(request.ItemId); await repository.UpdateAsync(sale, cancellationToken); await eventPublisher.PublishAsync(new ItemCancelled(sale.Id, request.ItemId), cancellationToken); return SaleMappings.ToResult(sale);
        }
    }
}
