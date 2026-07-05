using SqlSugar;

namespace WatchShop.Domain.Entities;

[SugarTable("category")]
public class Category : BaseEntity
{
    [SugarColumn(Length = 100, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(IsNullable = true)]
    public long? ParentId { get; set; }

    public int SortOrder { get; set; }

    public bool IsEnabled { get; set; } = true;
}
