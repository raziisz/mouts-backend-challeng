using Ambev.DeveloperEvaluation.Application.Carts;
using Ambev.DeveloperEvaluation.Application.Products;
using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Catalog;

[ApiController]
[Authorize]
[Route("products")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery(Name = "_page")] int page = 1, [FromQuery(Name = "_size")] int size = 10, [FromQuery(Name = "_order")] string? order = null, string? title = null, string? category = null, decimal? price = null, decimal? _minPrice = null, decimal? _maxPrice = null, CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new GetProductsQuery(page, size, order, title, category, price, _minPrice, _maxPrice), cancellationToken));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories(CancellationToken cancellationToken) => Ok(await mediator.Send(new GetProductCategoriesQuery(), cancellationToken));

    [HttpGet("category/{category}")]
    public Task<IActionResult> ByCategory(string category, [FromQuery(Name = "_page")] int page = 1, [FromQuery(Name = "_size")] int size = 10, [FromQuery(Name = "_order")] string? order = null, CancellationToken cancellationToken = default) => List(page, size, order, null, category, null, null, null, cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken) => Ok(await mediator.Send(new GetProductQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(ProductRequest request, CancellationToken cancellationToken) { var result = await mediator.Send(request.ToCommand(), cancellationToken); return CreatedAtAction(nameof(Get), new { id = result.Id }, result); }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, ProductRequest request, CancellationToken cancellationToken) => Ok(await mediator.Send(request.ToCommand(id), cancellationToken));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) { if (!await mediator.Send(new DeleteProductCommand(id), cancellationToken)) return NotFound(new ApiErrorResponse { Type = "ResourceNotFound", Error = "Resource not found", Detail = $"Product {id} was not found." }); return Ok(new { message = "Product deleted successfully" }); }

}

public sealed class ProductRequest
{
    public string Title { get; set; } = string.Empty; public decimal Price { get; set; } public string Description { get; set; } = string.Empty; public string Category { get; set; } = string.Empty; public string Image { get; set; } = string.Empty; public ProductRatingRequest Rating { get; set; } = new();
    public CreateProductCommand ToCommand() => new(Title, Price, Description, Category, Image, Rating.Rate, Rating.Count);
    public UpdateProductCommand ToCommand(int id) => new(id, Title, Price, Description, Category, Image, Rating.Rate, Rating.Count);
}
public sealed class ProductRatingRequest { public decimal Rate { get; set; } public int Count { get; set; } }

[ApiController]
[Authorize]
[Route("carts")]
public class CartsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery(Name = "_page")] int page = 1, [FromQuery(Name = "_size")] int size = 10, [FromQuery(Name = "_order")] string? order = null, int? userId = null, DateTime? date = null, DateTime? _minDate = null, DateTime? _maxDate = null, CancellationToken cancellationToken = default)
    {
        if (!User.IsPrivileged()) userId = User.GetRequiredUserId();
        return Ok(await mediator.Send(new GetCartsQuery(page, size, order, userId, date, _minDate, _maxDate), cancellationToken));
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCartQuery(id), cancellationToken);
        if (!User.IsPrivileged() && result.UserId != User.GetRequiredUserId()) return Forbid();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CartRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsPrivileged() && request.UserId != User.GetRequiredUserId()) return Forbid();
        var result = await mediator.Send(request.ToCreate(), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CartRequest request, CancellationToken cancellationToken)
    {
        var current = await mediator.Send(new GetCartQuery(id), cancellationToken);
        if (!User.IsPrivileged() && (current.UserId != User.GetRequiredUserId() || request.UserId != current.UserId)) return Forbid();
        return Ok(await mediator.Send(request.ToUpdate(id), cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var current = await mediator.Send(new GetCartQuery(id), cancellationToken);
        if (!User.IsPrivileged() && current.UserId != User.GetRequiredUserId()) return Forbid();
        if (!await mediator.Send(new DeleteCartCommand(id), cancellationToken)) return NotFound(new ApiErrorResponse { Type = "ResourceNotFound", Error = "Resource not found", Detail = $"Cart {id} was not found." });
        return Ok(new { message = "Cart deleted successfully" });
    }
}
public sealed class CartRequest
{
    public int UserId { get; set; } public DateTime Date { get; set; } public List<CartItemResult> Products { get; set; } = [];
    public CreateCartCommand ToCreate() => new(UserId, Date, Products); public UpdateCartCommand ToUpdate(int id) => new(id, UserId, Date, Products);
}

[ApiController]
[Authorize]
[Route("sales")]
public class SalesController(IMediator mediator) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List([FromQuery(Name = "_page")] int page = 1, [FromQuery(Name = "_size")] int size = 10, [FromQuery(Name = "_order")] string? order = null, string? saleNumber = null, string? status = null, DateTime? date = null, DateTime? _minDate = null, DateTime? _maxDate = null, CancellationToken cancellationToken = default)
    {
        int? customerId = User.IsPrivileged() ? null : User.GetRequiredUserId();
        return Ok(await mediator.Send(new GetSalesQuery(page, size, order, saleNumber, status, date, _minDate, _maxDate, customerId), cancellationToken));
    }
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSaleQuery(id), cancellationToken);
        if (!User.IsPrivileged() && result.Customer.Id != User.GetRequiredUserId()) return Forbid();
        return Ok(result);
    }

    [HttpPost] public async Task<IActionResult> Create(SaleRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsPrivileged() && request.Customer.Id != User.GetRequiredUserId()) return Forbid();
        var result = await mediator.Send(request.ToCreate(), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, SaleRequest request, CancellationToken cancellationToken) => Ok(await mediator.Send(request.ToUpdate(id), cancellationToken));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) => Ok(await mediator.Send(new CancelSaleCommand(id), cancellationToken));

    [HttpPatch("{saleId:int}/items/{itemId:int}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CancelItem(int saleId, int itemId, CancellationToken cancellationToken) => Ok(await mediator.Send(new CancelSaleItemCommand(saleId, itemId), cancellationToken));

}
public sealed class SaleRequest
{
    public string SaleNumber { get; set; } = string.Empty; public DateTime Date { get; set; } public ExternalIdentity Customer { get; set; } = new(); public ExternalIdentity Branch { get; set; } = new(); public List<SaleItemInputModel> Products { get; set; } = [];
    public CreateSaleCommand ToCreate() => new(SaleNumber, Date, Customer.Id, Customer.Description, Branch.Id, Branch.Description, Products);
    public UpdateSaleCommand ToUpdate(int id) => new(id, SaleNumber, Date, Customer.Id, Customer.Description, Branch.Id, Branch.Description, Products);
}
