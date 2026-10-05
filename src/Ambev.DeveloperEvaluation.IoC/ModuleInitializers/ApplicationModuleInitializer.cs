using Ambev.DeveloperEvaluation.Application.Carts;
using Ambev.DeveloperEvaluation.Application.Products;
using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Common.Security;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class ApplicationModuleInitializer : IModuleInitializer
{
    public void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        builder.Services.AddTransient<IValidator<CreateProductCommand>, CreateProductCommandValidator>();
        builder.Services.AddTransient<IValidator<UpdateProductCommand>, UpdateProductCommandValidator>();
        builder.Services.AddTransient<IValidator<GetProductsQuery>, GetProductsQueryValidator>();
        builder.Services.AddTransient<IValidator<GetProductQuery>, GetProductQueryValidator>();
        builder.Services.AddTransient<IValidator<DeleteProductCommand>, DeleteProductCommandValidator>();

        builder.Services.AddTransient<IValidator<CreateCartCommand>, CreateCartCommandValidator>();
        builder.Services.AddTransient<IValidator<UpdateCartCommand>, UpdateCartCommandValidator>();
        builder.Services.AddTransient<IValidator<GetCartsQuery>, GetCartsQueryValidator>();
        builder.Services.AddTransient<IValidator<GetCartQuery>, GetCartQueryValidator>();
        builder.Services.AddTransient<IValidator<DeleteCartCommand>, DeleteCartCommandValidator>();

        builder.Services.AddTransient<IValidator<CreateSaleCommand>, CreateSaleCommandValidator>();
        builder.Services.AddTransient<IValidator<UpdateSaleCommand>, UpdateSaleCommandValidator>();
        builder.Services.AddTransient<IValidator<GetSalesQuery>, GetSalesQueryValidator>();
        builder.Services.AddTransient<IValidator<GetSaleQuery>, GetSaleQueryValidator>();
        builder.Services.AddTransient<IValidator<CancelSaleCommand>, CancelSaleCommandValidator>();
        builder.Services.AddTransient<IValidator<CancelSaleItemCommand>, CancelSaleItemCommandValidator>();
    }
}
