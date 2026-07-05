using FluentValidation.TestHelper;
using WatchShop.Application.Features.Brands.Dtos;
using WatchShop.Application.Features.Validators;

namespace WatchShop.Tests;

public class ValidatorTests
{
    [Fact]
    public void BrandCreateRequest_EmptyName_ShouldFail()
    {
        var validator = new BrandCreateRequestValidator();
        var result = validator.TestValidate(new BrandCreateRequest { Name = "" });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CustomerRegisterRequest_ShortPassword_ShouldFail()
    {
        var validator = new CustomerRegisterRequestValidator();
        var result = validator.TestValidate(new Application.Features.Store.Dtos.CustomerRegisterRequest
        {
            Username = "user1",
            Password = "123"
        });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
