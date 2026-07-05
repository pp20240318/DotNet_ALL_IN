using SqlSugar;

namespace WatchShop.Domain.Entities;

public abstract class BaseEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public long Id { get; set; }

    public bool IsDeleted { get; set; }

    [SugarColumn(IsEnableUpdateVersionValidation = true)]
    public int Version { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [SugarColumn(IsNullable = true)]
    public DateTime? UpdatedAt { get; set; }
}
