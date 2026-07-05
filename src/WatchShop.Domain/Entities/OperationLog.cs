using SqlSugar;

namespace WatchShop.Domain.Entities;

[SugarTable("operation_log")]
public class OperationLog : BaseEntity
{
    public long? AdminId { get; set; }

    [SugarColumn(Length = 50, IsNullable = false)]
    public string AdminName { get; set; } = string.Empty;

    [SugarColumn(Length = 50, IsNullable = false)]
    public string Module { get; set; } = string.Empty;

    [SugarColumn(Length = 50, IsNullable = false)]
    public string Action { get; set; } = string.Empty;

    [SugarColumn(Length = 200, IsNullable = true)]
    public string? RequestPath { get; set; }

    [SugarColumn(Length = 10, IsNullable = true)]
    public string? RequestMethod { get; set; }

    [SugarColumn(Length = 50, IsNullable = true)]
    public string? IpAddress { get; set; }

    public bool IsSuccess { get; set; }

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Message { get; set; }
}
