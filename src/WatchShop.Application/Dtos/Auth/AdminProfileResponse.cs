namespace WatchShop.Application.Dtos.Auth;

public class AdminProfileResponse
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
