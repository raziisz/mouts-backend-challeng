using Ambev.DeveloperEvaluation.Common.Persistence;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public sealed record ProductListQuery(PageQuery Page, string? Title = null, string? Category = null, decimal? Price = null, decimal? MinPrice = null, decimal? MaxPrice = null);

public sealed record CartListQuery(PageQuery Page, int? UserId = null, DateTime? Date = null, DateTime? MinDate = null, DateTime? MaxDate = null);

public sealed record SaleListQuery(PageQuery Page, string? SaleNumber = null, string? Status = null, DateTime? Date = null, DateTime? MinDate = null, DateTime? MaxDate = null, int? CustomerId = null);

public sealed record UserListQuery(PageQuery Page, string? Username = null, string? Email = null, string? Phone = null, UserStatus? Status = null, UserRole? Role = null);
