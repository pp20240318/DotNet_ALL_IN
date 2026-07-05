using FluentValidation;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Dtos.Auth;
using WatchShop.Application.Features.Brands.Dtos;
using WatchShop.Application.Features.Catalog.Dtos;
using WatchShop.Application.Features.Store.Dtos;

namespace WatchShop.Application.Features.Validators;

public class BrandCreateRequestValidator : AbstractValidator<BrandCreateRequest>
{
    public BrandCreateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CategoryCreateRequestValidator : AbstractValidator<CategoryCreateRequest>
{
    public CategoryCreateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class ProductCreateRequestValidator : AbstractValidator<ProductCreateRequest>
{
    public ProductCreateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BrandId).GreaterThan(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public class CustomerRegisterRequestValidator : AbstractValidator<CustomerRegisterRequest>
{
    public CustomerRegisterRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(50);
    }
}

public class CustomerLoginRequestValidator : AbstractValidator<CustomerLoginRequest>
{
    public CustomerLoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class CreateBrandCommandValidator : AbstractValidator<Brands.Commands.CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new BrandCreateRequestValidator());
    }
}

public class CreateCategoryCommandValidator : AbstractValidator<Categories.CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new CategoryCreateRequestValidator());
    }
}

public class CreateProductCommandValidator : AbstractValidator<Products.CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new ProductCreateRequestValidator());
    }
}

public class AdminLoginCommandValidator : AbstractValidator<Auth.AdminLoginCommand>
{
    public AdminLoginCommandValidator()
    {
        RuleFor(x => x.Request.Username).NotEmpty();
        RuleFor(x => x.Request.Password).NotEmpty();
    }
}

public class UpdateBrandCommandValidator : AbstractValidator<Brands.Commands.UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.Request).SetValidator(new BrandCreateRequestValidator());
    }
}

public class CreateSkuCommandValidator : AbstractValidator<ProductSkus.CreateSkuCommand>
{
    public CreateSkuCommandValidator()
    {
        RuleFor(x => x.Request.ProductId).GreaterThan(0);
        RuleFor(x => x.Request.SkuCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Stock).GreaterThanOrEqualTo(0);
    }
}

public class StoreOrderCreateRequestValidator : AbstractValidator<StoreOrderCreateRequest>
{
    public StoreOrderCreateRequestValidator()
    {
        RuleFor(x => x.SkuId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.ReceiverName).NotEmpty();
        RuleFor(x => x.ReceiverPhone).NotEmpty();
        RuleFor(x => x.ReceiverAddress).NotEmpty();
    }
}

public class CartAddRequestValidator : AbstractValidator<CartAddRequest>
{
    public CartAddRequestValidator()
    {
        RuleFor(x => x.SkuId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
