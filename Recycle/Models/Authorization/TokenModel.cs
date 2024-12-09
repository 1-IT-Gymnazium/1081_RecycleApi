using System.ComponentModel.DataAnnotations;

namespace Recycle.Api.Models.Authorization;

public class TokenModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Required]
    public string Token { get; set; } = null!;
}
