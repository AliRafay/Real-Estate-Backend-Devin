using System.ComponentModel.DataAnnotations;

namespace Demo.WebApi.Application.Identity.Tokens;

public class TokenRequest : DeviceInfoRequest
{
    [Required]
    public string Email { get; set; } = default!;
    [Required]
    public string Password { get; set; } = default!;
};