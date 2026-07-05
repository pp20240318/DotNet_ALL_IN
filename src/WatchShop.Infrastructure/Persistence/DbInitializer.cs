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
            typeof(Customer),
            typeof(Brand),
            typeof(Category),
            typeof(Product),
            typeof(ProductSku),
            typeof(CartItem),
            typeof(ShopOrder),
            typeof(OrderItem),
            typeof(OperationLog));

        SeedAdminUser();
        SeedCustomerUser();
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

    private void SeedCustomerUser()
    {
        const string demoUsername = "demo";
        if (_db.Queryable<Customer>().Any(x => x.Username == demoUsername))
        {
            return;
        }

        _db.Insertable(new Customer
        {
            Username = demoUsername,
            PasswordHash = PasswordHasher.Hash("Demo@123"),
            Nickname = "演示用户",
            Phone = "13800138000",
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        }).ExecuteCommand();
    }

    private void SeedCatalogData()
    {
        if (_db.Queryable<Brand>().Any())
        {
            SeedSkusIfMissing();
            return;
        }

        var rolexId = InsertBrand("Rolex", "瑞士劳力士");
        var omegaId = InsertBrand("Omega", "瑞士欧米茄");
        var mechanicalId = InsertCategory("机械表");
        var quartzId = InsertCategory("石英表");
        var p1 = InsertProduct("Rolex Submariner", rolexId, mechanicalId, 88000m);
        var p2 = InsertProduct("Omega Seamaster", omegaId, mechanicalId, 52000m);
        var p3 = InsertProduct("Casio Classic", omegaId, quartzId, 899m);
        InsertSku(p1, "SUB-BLK-001", "{\"color\":\"黑\"}", 88000m, 50);
        InsertSku(p2, "SEA-BLU-001", "{\"color\":\"蓝\"}", 52000m, 30);
        InsertSku(p3, "CAS-SLV-001", "{\"color\":\"银\"}", 899m, 200);
    }

    private void SeedSkusIfMissing()
    {
        if (_db.Queryable<ProductSku>().Any())
        {
            return;
        }

        var products = _db.Queryable<Product>().Where(x => !x.IsDeleted).ToList();
        foreach (var product in products)
        {
            InsertSku(product.Id, $"{product.Name[..Math.Min(3, product.Name.Length)].ToUpper()}-001",
                "{\"default\":true}", product.Price, 100);
        }
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

    private long InsertProduct(string name, long brandId, long categoryId, decimal price)
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
        return entity.Id;
    }

    private void InsertSku(long productId, string skuCode, string specJson, decimal price, int stock)
    {
        _db.Insertable(new ProductSku
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            ProductId = productId,
            SkuCode = skuCode,
            SpecJson = specJson,
            Price = price,
            Stock = stock,
            IsEnabled = true,
            IsDeleted = false,
            Version = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ExecuteCommand();
    }
}
