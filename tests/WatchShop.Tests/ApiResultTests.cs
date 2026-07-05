using WatchShop.Application.Common;

namespace WatchShop.Tests;

public class ApiResultTests
{
    [Fact]
    public void Ok_Should_Set_Success_Code()
    {
        var result = ApiResult<string>.Ok("data");
        Assert.Equal(0, result.Code);
        Assert.Equal("success", result.Message);
        Assert.Equal("data", result.Data);
    }

    [Fact]
    public void Fail_Should_Set_Error_Code()
    {
        var result = ApiResult<object>.Fail(404, "not found");
        Assert.Equal(404, result.Code);
        Assert.Equal("not found", result.Message);
    }
}
