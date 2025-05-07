using System.ComponentModel.DataAnnotations;

namespace Demo.WebApi.Application.Identity.Tokens;

public class RefreshTokenRequest : DeviceInfoRequest
{
    [Required]
    public string Token { get; set; }
    [Required]
    public string RefreshToken { get; set; }
}