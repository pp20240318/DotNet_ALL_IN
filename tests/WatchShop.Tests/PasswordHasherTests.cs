using WatchShop.Infrastructure.Security;

namespace WatchShop.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_And_Verify_Should_Work()
    {
        var hash = PasswordHasher.Hash("Admin@123");
        Assert.True(PasswordHasher.Verify("Admin@123", hash));
        Assert.False(PasswordHasher.Verify("wrong", hash));
    }
}
