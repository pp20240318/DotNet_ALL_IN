using SqlSugar;

namespace WatchShop.Domain.Entities;

[SugarTable("brand")]
public class Brand : BaseEntity
{
    [SugarColumn(Length = 100, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? LogoUrl { get; set; }

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsEnabled { get; set; } = true;
}
