using Ambev.DeveloperEvaluation.Application.Carts;
using Ambev.DeveloperEvaluation.Application.Products;
using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Catalog;

[ApiController]
[Route("products")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery(Name = "_page")] int page = 1, [FromQuery(Name = "_size")] int size = 10, [FromQuery(Name = "_order")] string? order = null, string? title = null, string? category = null, decimal? price = null, decimal? _minPrice = null, decimal? _maxPrice = null, CancellationToken cancellationToken = default)
    {
        var products = (await mediator.Send(new GetProductsQuery(), cancellationToken)).AsEnumerable();
        if (!string.IsNullOrWhiteSpace(title)) products = products.Where(x => TextFilter.Matches(x.Title, title));
        if (!string.IsNullOrWhiteSpace(category)) products = products.Where(x => TextFilter.Matches(x.Category, category));
        if (price.HasValue) products = products.Where(x => x.Price == price.Value);
        if (_minPrice.HasValue) products = products.Where(x => x.Price >= _minPrice.Value);
        if (_maxPrice.HasValue) products = products.Where(x => x.Price <= _maxPrice.Value);
        products = ApplyOrder(products, order);
        return Ok(PagedResultFactory.Create(products, page, size));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories(CancellationToken cancellationToken) => Ok((await mediator.Send(new GetProductsQuery(), cancellationToken)).Select(x => x.Category).Distinct().OrderBy(x => x));

    [HttpGet("category/{category}")]
    public Task<IActionResult> ByCategory(string category, [FromQuery(Name = "_page")] int page = 1, [FromQuery(Name = "_size")] int size = 10, [FromQuery(Name = "_order")] string? order = null, CancellationToken cancellationToken = default) => List(page, size, order, null, category, null, null, null, cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken) => Ok(await mediator.Send(new GetProductQuery(id), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(ProductRequest request, CancellationToken cancellationToken) { var result = await mediator.Send(request.ToCommand(), cancellationToken); return CreatedAtAction(nameof(Get), new { id = result.Id }, result); }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductRequest request, CancellationToken cancellationToken) => Ok(await mediator.Send(request.ToCommand(id), cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) { if (!await mediator.Send(new DeleteProductCommand(id), cancellationToken)) return NotFound(); return Ok(new { message = "Product deleted successfully" }); }

    private static IEnumerable<ProductResult> ApplyOrder(IEnumerable<ProductResult> source, string? order)
    {
        if (string.IsNullOrWhiteSpace(order)) return source.OrderBy(x => x.Id);
        IOrderedEnumerable<ProductResult>? result = null;
        foreach (var part in order.Replace("\"", string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries); var desc = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase); var field = tokens[0].ToLowerInvariant();
            result = result is null ? Order(source, field, desc) : ThenOrder(result, field, desc);
        }
        return result ?? source;
    }
    private static IOrderedEnumerable<ProductResult> Order(IEnumerable<ProductResult> source, string field, bool desc) => field switch { "price" => desc ? source.OrderByDescending(x => x.Price) : source.OrderBy(x => x.Price), "title" => desc ? source.OrderByDescending(x => x.Title) : source.OrderBy(x => x.Title), _ => desc ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id) };
    private static IOrderedEnumerable<ProductResult> ThenOrder(IOrderedEnumerable<ProductResult> source, string field, bool desc) => field switch { "price" => desc ? source.ThenByDescending(x => x.Price) : source.ThenBy(x => x.Price), "title" => desc ? source.ThenByDescending(x => x.Title) : source.ThenBy(x => x.Title), _ => desc ? source.ThenByDescending(x => x.Id) : source.ThenBy(x => x.Id) };
}

public sealed class ProductRequest
{
    public string Title { get; set; } = string.Empty; public decimal Price { get; set; } public string Description { get; set; } = string.Empty; public string Category { get; set; } = string.Empty; public string Image { get; set; } = string.Empty; public ProductRatingRequest Rating { get; set; } = new();
    public CreateProductCommand ToCommand() => new(Title, Price, Description, Category, Image, Rating.Rate, Rating.Count);
    public UpdateProductCommand ToCommand(int id) => new(id, Title, Price, Description, Category, Image, Rating.Rate, Rating.Count);
}
public sealed class ProductRatingRequest { public decimal Rate { get; set; } public int Count { get; set; } }

[ApiController]
[Route("carts")]
public class CartsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery(Name = "_page")] int page = 1, [FromQuery(Name = "_size")] int size = 10, [FromQuery(Name = "_order")] string? order = null, int? userId = null, DateTime? date = null, DateTime? _minDate = null, DateTime? _maxDate = null, CancellationToken cancellationToken = default)
    {
        var carts = (await mediator.Send(new GetCartsQuery(), cancellationToken)).AsEnumerable();
        if (userId.HasValue) carts = carts.Where(x => x.UserId == userId.Value);
        if (date.HasValue) carts = carts.Where(x => x.Date.Date == date.Value.Date);
        if (_minDate.HasValue) carts = carts.Where(x => x.Date >= _minDate.Value);
        if (_maxDate.HasValue) carts = carts.Where(x => x.Date <= _maxDate.Value);
        carts = ApplyOrder(carts, order);
        return Ok(PagedResultFactory.Create(carts, page, size));
    }
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id, CancellationToken cancellationToken) => Ok(await mediator.Send(new GetCartQuery(id), cancellationToken));
    [HttpPost] public async Task<IActionResult> Create(CartRequest request, CancellationToken cancellationToken) { var result = await mediator.Send(request.ToCreate(), cancellationToken); return CreatedAtAction(nameof(Get), new { id = result.Id }, result); }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, CartRequest request, CancellationToken cancellationToken) => Ok(await mediator.Send(request.ToUpdate(id), cancellationToken));
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) { if (!await mediator.Send(new DeleteCartCommand(id), cancellationToken)) return NotFound(); return Ok(new { message = "Cart deleted successfully" }); }
    private static IEnumerable<CartResult> ApplyOrder(IEnumerable<CartResult> source, string? order)
    {
        if (string.IsNullOrWhiteSpace(order)) return source.OrderBy(x => x.Id);
        IOrderedEnumerable<CartResult>? result = null;
        foreach (var part in order.Replace("\"", string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var desc = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            var field = tokens[0].ToLowerInvariant();
            result = result is null ? Order(source, field, desc) : ThenOrder(result, field, desc);
        }
        return result ?? source;
    }

    private static IOrderedEnumerable<CartResult> Order(IEnumerable<CartResult> source, string field, bool desc) => field switch
    {
        "userid" => desc ? source.OrderByDescending(x => x.UserId) : source.OrderBy(x => x.UserId),
        "date" => desc ? source.OrderByDescending(x => x.Date) : source.OrderBy(x => x.Date),
        _ => desc ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
    };

    private static IOrderedEnumerable<CartResult> ThenOrder(IOrderedEnumerable<CartResult> source, string field, bool desc) => field switch
    {
        "userid" => desc ? source.ThenByDescending(x => x.UserId) : source.ThenBy(x => x.UserId),
        "date" => desc ? source.ThenByDescending(x => x.Date) : source.ThenBy(x => x.Date),
        _ => desc ? source.ThenByDescending(x => x.Id) : source.ThenBy(x => x.Id)
    };
}
public sealed class CartRequest
{
    public int UserId { get; set; } public DateTime Date { get; set; } public List<CartItemResult> Products { get; set; } = [];
    public CreateCartCommand ToCreate() => new(UserId, Date, Products); public UpdateCartCommand ToUpdate(int id) => new(id, UserId, Date, Products);
}

[ApiController]
[Route("sales")]
public class SalesController(IMediator mediator) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List([FromQuery(Name = "_page")] int page = 1, [FromQuery(Name = "_size")] int size = 10, [FromQuery(Name = "_order")] string? order = null, string? saleNumber = null, string? status = null, DateTime? date = null, DateTime? _minDate = null, DateTime? _maxDate = null, CancellationToken cancellationToken = default)
    {
        var sales = (await mediator.Send(new GetSalesQuery(), cancellationToken)).AsEnumerable();
        if (!string.IsNullOrWhiteSpace(saleNumber)) sales = sales.Where(x => TextFilter.Matches(x.SaleNumber, saleNumber));
        if (!string.IsNullOrWhiteSpace(status)) sales = sales.Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        if (date.HasValue) sales = sales.Where(x => x.Date.Date == date.Value.Date);
        if (_minDate.HasValue) sales = sales.Where(x => x.Date >= _minDate.Value);
        if (_maxDate.HasValue) sales = sales.Where(x => x.Date <= _maxDate.Value);
        sales = ApplyOrder(sales, order);
        return Ok(PagedResultFactory.Create(sales, page, size));
    }
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id, CancellationToken cancellationToken) => Ok(await mediator.Send(new GetSaleQuery(id), cancellationToken));
    [HttpPost] public async Task<IActionResult> Create(SaleRequest request, CancellationToken cancellationToken) { var result = await mediator.Send(request.ToCreate(), cancellationToken); return CreatedAtAction(nameof(Get), new { id = result.Id }, result); }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, SaleRequest request, CancellationToken cancellationToken) => Ok(await mediator.Send(request.ToUpdate(id), cancellationToken));
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) => Ok(await mediator.Send(new CancelSaleCommand(id), cancellationToken));
    [HttpPatch("{saleId:int}/items/{itemId:int}/cancel")] public async Task<IActionResult> CancelItem(int saleId, int itemId, CancellationToken cancellationToken) => Ok(await mediator.Send(new CancelSaleItemCommand(saleId, itemId), cancellationToken));

    private static IEnumerable<SaleResult> ApplyOrder(IEnumerable<SaleResult> source, string? order)
    {
        if (string.IsNullOrWhiteSpace(order)) return source.OrderByDescending(x => x.Date);
        IOrderedEnumerable<SaleResult>? result = null;
        foreach (var part in order.Replace("\"", string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var desc = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            var field = tokens[0].ToLowerInvariant();
            result = result is null ? Order(source, field, desc) : ThenOrder(result, field, desc);
        }
        return result ?? source;
    }

    private static IOrderedEnumerable<SaleResult> Order(IEnumerable<SaleResult> source, string field, bool desc) => field switch
    {
        "salenumber" => desc ? source.OrderByDescending(x => x.SaleNumber) : source.OrderBy(x => x.SaleNumber),
        "totalamount" => desc ? source.OrderByDescending(x => x.TotalAmount) : source.OrderBy(x => x.TotalAmount),
        "status" => desc ? source.OrderByDescending(x => x.Status) : source.OrderBy(x => x.Status),
        "date" => desc ? source.OrderByDescending(x => x.Date) : source.OrderBy(x => x.Date),
        _ => desc ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
    };

    private static IOrderedEnumerable<SaleResult> ThenOrder(IOrderedEnumerable<SaleResult> source, string field, bool desc) => field switch
    {
        "salenumber" => desc ? source.ThenByDescending(x => x.SaleNumber) : source.ThenBy(x => x.SaleNumber),
        "totalamount" => desc ? source.ThenByDescending(x => x.TotalAmount) : source.ThenBy(x => x.TotalAmount),
        "status" => desc ? source.ThenByDescending(x => x.Status) : source.ThenBy(x => x.Status),
        "date" => desc ? source.ThenByDescending(x => x.Date) : source.ThenBy(x => x.Date),
        _ => desc ? source.ThenByDescending(x => x.Id) : source.ThenBy(x => x.Id)
    };
}
public sealed class SaleRequest
{
    public string SaleNumber { get; set; } = string.Empty; public DateTime Date { get; set; } public ExternalIdentity Customer { get; set; } = new(); public ExternalIdentity Branch { get; set; } = new(); public List<SaleItemInputModel> Products { get; set; } = [];
    public CreateSaleCommand ToCreate() => new(SaleNumber, Date, Customer.Id, Customer.Description, Branch.Id, Branch.Description, Products);
    public UpdateSaleCommand ToUpdate(int id) => new(id, SaleNumber, Date, Customer.Id, Customer.Description, Branch.Id, Branch.Description, Products);
}
