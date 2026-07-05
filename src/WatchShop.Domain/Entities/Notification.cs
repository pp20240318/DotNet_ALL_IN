using SqlSugar;

namespace WatchShop.Domain.Entities;

[SugarTable("notification")]
public class Notification : BaseEntity
{
    [SugarColumn(Length = 100, IsNullable = false)]
    public string Title { get; set; } = string.Empty;

    [SugarColumn(Length = 500, IsNullable = false)]
    public string Content { get; set; } = string.Empty;

    [SugarColumn(Length = 30, IsNullable = false)]
    public string Category { get; set; } = "system";

    public long? RelatedId { get; set; }

    public bool IsRead { get; set; }
}
