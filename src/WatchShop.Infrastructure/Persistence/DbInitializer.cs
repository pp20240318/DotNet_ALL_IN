using SqlSugar;
using WatchShop.Domain.Entities;
using WatchShop.Infrastructure.Security;

namespace WatchShop.Infrastructure.Persistence;

public class DbInitializer
{
    private readonly ISqlSugarClient _db;

    public DbInitializer(ISqlSugarClient db)
    {
        _db = db;
    }

    public void Initialize()
    {
        _db.CodeFirst.InitTables(
            typeof(Admin),
            typeof(Brand),
            typeof(Category),
            typeof(Product),
            typeof(ProductSku),
            typeof(ShopOrder),
            typeof(OrderItem),
            typeof(OperationLog));

        SeedAdminUser();
        SeedCatalogData();
    }

    private void SeedAdminUser()
    {
        const string defaultUsername = "admin";
        if (_db.Queryable<Admin>().Any(x => x.Username == defaultUsername))
        {
            return;
        }

        _db.Insertable(new Admin
        {
            Username = defaultUsername,
            PasswordHash = PasswordHasher.Hash("Admin@123"),
            DisplayName = "系统管理员",
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        }).ExecuteCommand();
    }

    private void SeedCatalogData()
    {
        if (_db.Queryable<Brand>().Any())
        {
            return;
        }

        var rolexId = InsertBrand("Rolex", "瑞士劳力士");
        var omegaId = InsertBrand("Omega", "瑞士欧米茄");
        var mechanicalId = InsertCategory("机械表");
        var quartzId = InsertCategory("石英表");
        InsertProduct("Rolex Submariner", rolexId, mechanicalId, 88000m);
        InsertProduct("Omega Seamaster", omegaId, mechanicalId, 52000m);
        InsertProduct("Casio Classic", omegaId, quartzId, 899m);
    }

    private long InsertBrand(string name, string description)
    {
        var entity = new Brand
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            Name = name,
            Description = description,
            SortOrder = 1,
            IsEnabled = true,
            IsDeleted = false,
            Version = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Insertable(entity).ExecuteCommand();
        return entity.Id;
    }

    private long InsertCategory(string name)
    {
        var entity = new Category
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            Name = name,
            SortOrder = 1,
            IsEnabled = true,
            IsDeleted = false,
            Version = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Insertable(entity).ExecuteCommand();
        return entity.Id;
    }

    private void InsertProduct(string name, long brandId, long categoryId, decimal price)
    {
        var entity = new Product
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            Name = name,
            BrandId = brandId,
            CategoryId = categoryId,
            Description = $"{name} 官方正品",
            Price = price,
            Status = Domain.Enums.ProductStatus.OnSale,
            IsDeleted = false,
            Version = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Insertable(entity).ExecuteCommand();
    }
}
