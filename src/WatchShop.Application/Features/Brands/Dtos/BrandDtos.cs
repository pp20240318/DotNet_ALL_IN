namespace WatchShop.Application.Features.Brands.Dtos;

public class BrandCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class BrandUpdateRequest : BrandCreateRequest { }

public class BrandResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; }
    public int Version { get; set; }
}
