using System.ComponentModel.DataAnnotations;

namespace Recycle.Api.Models.Authorization;

/// <summary>
/// Data required to validate a user's email confirmation token.
/// </summary>
public class TokenModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Required]
    public string Token { get; set; } = null!;
}
