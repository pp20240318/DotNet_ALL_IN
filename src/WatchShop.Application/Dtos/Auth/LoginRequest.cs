using System.ComponentModel.DataAnnotations;

namespace WatchShop.Application.Dtos.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "用户名不能为空")]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密码不能为空")]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
